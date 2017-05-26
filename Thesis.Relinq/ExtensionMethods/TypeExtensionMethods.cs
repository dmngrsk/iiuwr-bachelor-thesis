using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Thesis.Relinq
{
    public static partial class ExtensionMethods
    {
        public static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive 
                || typeInfo.IsEnum
                || typeInfo.Equals(typeof(string))
                || typeInfo.Equals(typeof(decimal));
        }

        public static bool IsAnonymous(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && typeInfo.IsGenericType
                && typeInfo.Name.Contains("AnonymousType")
                && (typeInfo.Name.StartsWith("<>") || typeInfo.Name.StartsWith("VB$"))
                && typeInfo.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        public static PropertyInfo[] GetPublicSettableProperties(this Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetSetMethod() != null)
                .ToArray();
        }
    }
}