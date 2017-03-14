using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Thesis.Relinq
{
    public class NpgsqlRowConverter<T>
    {
        private static PropertyInfo[] _propertiesInfo = typeof(T).GetProperties();
        private static ReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> _columns;

        public NpgsqlRowConverter(ReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> columns)
        {
            _columns = columns;
        }

        public T ConvertArrayToObject(object[] row)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
                    
            for (int i = 0; i < _columns.Count; i++)
            {
                var prop = typeof(T).GetProperty(_columns[i].ColumnName);
                var propType = prop.PropertyType;
                
                try {
                    prop.SetValue(obj, Convert.ChangeType(row[i], propType));
                } catch (InvalidCastException) { /* Value is null */ }
            }

            return obj;
        }
    }
}