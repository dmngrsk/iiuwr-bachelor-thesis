using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Thesis.Relinq
{
    public static partial class ExtensionMethods
    {
        /// Checks whether the specified type is either a primitive, an enum, a string or a decimal.
        public static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive 
                || typeInfo.IsEnum
                || typeInfo.Equals(typeof(string))
                || typeInfo.Equals(typeof(decimal));
        }

        /// Checks whether the specified type is anonymous.
        public static bool IsAnonymous(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && typeInfo.IsGenericType
                && typeInfo.Name.Contains("AnonymousType")
                && (typeInfo.Name.StartsWith("<>") || typeInfo.Name.StartsWith("VB$"))
                && typeInfo.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        /// Returns specified type's public and settable properties.
        public static PropertyInfo[] GetPublicSettableProperties(this Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetSetMethod() != null)
                .ToArray();
        }
    }
}