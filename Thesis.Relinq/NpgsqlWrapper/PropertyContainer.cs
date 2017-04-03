using System;
using System.Collections.Generic;
using System.Reflection;

namespace Thesis.Relinq.NpgsqlWrapper
{
    public class PropertyContainer<T>
    {
        private Type _type;
        private Dictionary<string, PropertyInfo> _properties;

        public PropertyContainer()
        {
            this._type = typeof(T);
            this._properties = new Dictionary<string, PropertyInfo>();

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                _properties[prop.Name] = prop;
            }
        }

        public PropertyInfo GetProperty(string name)
        {
            return _properties[name]; 
        }
    }
}