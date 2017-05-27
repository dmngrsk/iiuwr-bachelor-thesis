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
                .ToList();
            
            if (propertyTypes.All(x => x.IsSimple()))
            {
                return rows
                    .Select(row => row.Values.ToArray())
                    .Select(row => (T)Activator.CreateInstance(typeof(T), row));
            }
            else // TODO: Refactor
            {
                var groupedCollectionType = propertyTypes
                    .First(x => typeof(IEnumerable).IsAssignableFrom(x) && x != typeof(string));
                var groupedCollectionTypeIndex = propertyTypes.IndexOf(groupedCollectionType);

                var groupedType = groupedCollectionType.GetGenericArguments()[0];
                var groupedTypeProperties = groupedType.GetPublicSettableProperties();

                var result = new List<T>();
                var genericListType = typeof(List<>).MakeGenericType(groupedType);
                
                for (int i = 0; i < rows.Length; i++)
                {
                    List<object> convertableRow = new List<object>(rows[i].Select(r => r.Value));
                    var groupedItemsCount = (long)rows[i][groupedType.Name + ".__GROUP_COUNT"];
                    var groupedItems = (IList)Activator.CreateInstance(genericListType);

                    if (groupedItemsCount > 0)
                    {
                        for (int j = 0; j < groupedItemsCount; j++)
                        {
                            var entity = Activator.CreateInstance(groupedType);

                            var entityProperties = rows[i + j]
                                .Where(x => x.Key.StartsWith(groupedType.Name) && !x.Key.EndsWith("__GROUP_COUNT"))
                                .ToDictionary(kvp => kvp.Key.Remove(0, groupedType.Name.Length + 1), kvp => kvp.Value);

                            foreach (var prop in groupedTypeProperties)
                            {
                                prop.SetValue(entity, entityProperties[prop.Name]);
                            }

                            groupedItems.Add(entity);
                        }

                        i += (int)(groupedItemsCount - 1);
                    }

                    convertableRow.RemoveRange(groupedCollectionTypeIndex, groupedTypeProperties.Count() + 1);
                    convertableRow.Insert(groupedCollectionTypeIndex, groupedItems);

                    result.Add((T)Activator.CreateInstance(typeof(T), convertableRow.ToArray()));
                }

                return result.AsEnumerable();
            }
        }
    }
}