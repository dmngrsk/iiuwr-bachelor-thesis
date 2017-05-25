using Dapper;
using System;
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
            var propertyTypes = typeof(T).GetProperties().Select(x => x.PropertyType);
            
            if (propertyTypes.All(x => x.IsSimple()))
            {
                return connection.Query(statement, parameters)
                    .Select(row => (IDictionary<string, object>)row)
                    .Select(row => row.Values.ToArray())
                    .Select(row => (T)Activator.CreateInstance(typeof(T), row));
            }
            
            var dapperRows = connection.Query(statement, parameters);
            var result = new List<T>();
            return result.AsEnumerable();
        }
    }
}