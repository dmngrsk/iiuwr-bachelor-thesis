using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Npgsql;

namespace Thesis.Relinq.NpgsqlWrapper
{
    public class NpgsqlRowConverter<T>
    {
        private static ReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> _columns;
        private static readonly PropertyContainer<T> _properties = new PropertyContainer<T>();

        public NpgsqlRowConverter(ReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> columns)
        {
            _columns = columns;
        }

        public static IEnumerable<T> ReadAllRows(NpgsqlConnection connection, string query)
        {
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = query;
            return ReadAllRows(connection, command);
        }

        public static IEnumerable<T> ReadAllRows(NpgsqlConnection connection, NpgsqlCommand command)
        {
            connection.Open();
            List<T> rows = new List<T>();

            using (var reader = command.ExecuteReader())
            {
                var columnSchema = reader.GetColumnSchema();
                var rowConverter = new NpgsqlRowConverter<T>(columnSchema);

                // TODO: Make this work properly!
                var typeIsAnonymous = typeof(T).Name.Contains("AnonymousType");

                while (reader.Read())
                {
                    var row = new object[reader.FieldCount];
                    reader.GetValues(row);

                    rows.Add(typeIsAnonymous ?
                        (T)Activator.CreateInstance(typeof(T), row) :
                        rowConverter.ConvertArrayToObject(row));
                }
            }

            connection.Close();
            command.Dispose();

            return rows;
        }

        private T ConvertArrayToObject(object[] row)
        {
            var typeInfo = typeof(T).GetTypeInfo();
            var typeIsSimple = (row.Length == 1) 
                && (typeInfo.IsPrimitive
                    || typeInfo.Equals(typeof(string))
                    || typeInfo.Equals(typeof(decimal))
                    || typeInfo.IsEnum);
            
            if (typeIsSimple)
            {
                return (T)Convert.ChangeType(row[0], typeof(T));
            }

            else
            {
                T obj = (T)Activator.CreateInstance(typeof(T));
                
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (row[i].GetType() != typeof(System.DBNull))
                    {
                        var prop = _properties.GetProperty(_columns[i].ColumnName);
                        var propType = prop.PropertyType;
                        
                        prop.SetValue(obj, Convert.ChangeType(row[i], propType));
                    }
                }

                return obj;
            }
        }
    }
}