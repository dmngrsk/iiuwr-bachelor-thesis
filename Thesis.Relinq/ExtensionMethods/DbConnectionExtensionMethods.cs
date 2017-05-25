using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Thesis.Relinq
{
    public static partial class ExtensionMethods
    {
        public static IEnumerable<T> QueryAnonymous<T>(this DbConnection connection, string statement)
        {
            return connection.QueryAnonymous<T>(statement, new Dictionary<string, object>());
        }

        public static IEnumerable<T> QueryAnonymous<T>(this DbConnection connection, 
            string statement, Dictionary<string, object> parameters)
        {
            var rows = connection.Query(statement, parameters)
                .Select(row => (IDictionary<string, object>)row)
                .ToArray();

            var propertyTypes = typeof(T)
                .GetProperties()
                .Select(x => x.PropertyType)
                .ToArray();
            
            if (propertyTypes.All(x => x.IsSimple()))
            {
                return rows
                    .Select(row => row.Values.ToArray())
                    .Select(row => (T)Activator.CreateInstance(typeof(T), row));
            }
            else
            {
                var groupedTypeCollectionIndex = propertyTypes
                    .TakeWhile(x => !x.Name.Contains("IEnumerable")).Count();

                var groupedType = propertyTypes[groupedTypeCollectionIndex]
                    .GetGenericArguments()[0];

                var groupedTypeProperties = groupedType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetSetMethod() != null);

                var result = new List<T>();
                var genericListType = typeof(List<>).MakeGenericType(groupedType);
                
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i > 800 || (string)rows[i]["CustomerID"] == "PARIS")
                    {
                        Console.Write("ho");
                    }

                    var groupedItemsCount = (long)rows[i][groupedType.Name + ".__GROUP_COUNT"];
                    var groupedItems = (IList)Activator.CreateInstance(genericListType);

                    for (int j = 0; j < groupedItemsCount; i++, j++)
                    {
                        var entity = Activator.CreateInstance(groupedType);

                        var entityProperties = rows[i]
                            .Where(x => x.Key.StartsWith(groupedType.Name) && !x.Key.EndsWith("__GROUP_COUNT"))
                            .ToDictionary(kvp => kvp.Key.Remove(0, groupedType.Name.Length + 1), kvp => kvp.Value);

                        foreach (var prop in groupedTypeProperties)
                        {
                            prop.SetValue(entity, entityProperties[prop.Name]);
                        }

                        groupedItems.Add(entity);
                    }

                    List<object> convertableRow = new List<object>(rows[i - 1].Select(r => r.Value));
                    convertableRow.RemoveRange(groupedTypeCollectionIndex, groupedTypeProperties.Count() + 1);
                    convertableRow.Insert(groupedTypeCollectionIndex, groupedItems);

                    result.Add((T)Activator.CreateInstance(typeof(T), convertableRow.ToArray()));
                }

                return result.AsEnumerable();
            }
        }
    }
}