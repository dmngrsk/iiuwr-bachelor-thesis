using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

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
            if (!_properties.ContainsKey(name))
            {
                foreach (var pair in _properties)
                {
                    if (MatchCase(name, pair.Key))
                    {
                        _properties[name] = pair.Value;
                        return _properties[name]; 
                    }
                }

                throw new ArgumentException($"Property {name} does not exist.");
            }

            return _properties[name]; 
        }

        private string _filter = @"[^a-zA-Z0-9]";

        private bool MatchCase(string parsable, string target) =>
            Regex.Replace(parsable, _filter, "").ToLower() == Regex.Replace(target, _filter, "").ToLower();
    }
}