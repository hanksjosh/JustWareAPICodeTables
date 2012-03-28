using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JustWareApiCodeTables
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, ReflectionInfo> _allReflectionInfo = new Dictionary<Type, ReflectionInfo>();

        public static ReflectionInfo GetReflectionInfo(this Type type)
        {
            ReflectionInfo reflectionInfo;
            if (!_allReflectionInfo.TryGetValue(type, out reflectionInfo))
            {
                reflectionInfo = new ReflectionInfo(type);
                _allReflectionInfo[type] = reflectionInfo;
            }

            return reflectionInfo;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesOfType<T>(this Type t)
        {
            Type targetType = typeof(T);
            return GetReflectionInfo(t).Properties.Where(p => targetType.IsAssignableFrom(p.PropertyType));
        }


        public static IEnumerable<Type> GetDerivedTypes(this Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(baseType.IsAssignableFrom);
        }

        public static IEnumerable<Type> GetDerivedTypes<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(typeof(T).IsAssignableFrom);
        }


        public static bool HasCustomAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>() != null;
        }

        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            return (T)type.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return (IEnumerable<T>)type.GetCustomAttributes(typeof(T), false);
        }

        public static bool HasCustomAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return propertyInfo.GetCustomAttribute<T>() != null;
        }

        public static T GetCustomAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (T)propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return (IEnumerable<T>)propertyInfo.GetCustomAttributes(typeof(T), false);
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(this Type type) where T : Attribute
        {
            return GetReflectionInfo(type).Properties.Where(p => p.HasCustomAttribute<T>());
        }

        public static PropertyInfo GetPropertyWithAttribute<T>(this Type type) where T : Attribute
        {
            return GetReflectionInfo(type).Properties.FirstOrDefault(p => p.HasCustomAttribute<T>());
        }

        public static IEnumerable<PropertyAndAttribute<T>> FindPropertiesWithAttribute<T>(this Type type) where T : Attribute
        {
            return GetReflectionInfo(type).Properties
                .Where(p => p.HasCustomAttribute<T>())
                .Select(p => new PropertyAndAttribute<T>(p, p.GetCustomAttributes<T>()));
        }

        public static object GetValue(this PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }

        public static T GetValue<T>(this PropertyInfo property, object obj)
        {
            return (T)GetValue(property, obj);
        }

        public static object CallGenericStaticMethod(Type type, Type genericType, string methodName, params object[] parameters)
        {
            var methodInfo = type.GetMethod(methodName, parameters.Select(p => p.GetType()).ToArray());
            var genericMethodInfo = methodInfo.MakeGenericMethod(genericType);
            return genericMethodInfo.Invoke(null, parameters);
        }

        public static IEnumerable<PropertyInfo> AssignableTo<T>(this IEnumerable<PropertyInfo> properties)
        {
            Type targetType = typeof(T);
            return properties.Where(p => targetType.IsAssignableFrom(p.PropertyType));
        }

        public static bool AssignableTo<T>(this PropertyInfo property)
        {
            return typeof(T).IsAssignableFrom(property.PropertyType);
        }

        public static IEnumerable<PropertyInfo> WithAttribute<T>(this IEnumerable<PropertyInfo> properties) where T : Attribute
        {
            return properties.Where(p => p.HasCustomAttribute<T>());
        }

        public class PropertyAndAttribute<T> where T : Attribute
        {
            public PropertyInfo Property { get; private set; }
            public IEnumerable<T> Attributes { get; private set; }

            public PropertyAndAttribute(PropertyInfo property, IEnumerable<T> attributes)
            {
                Property = property;
                Attributes = attributes;
            }
        }

        public static bool AllPropertiesEqual(this object a, object b, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            var propsA = a.GetType().GetProperties(flags).OrderBy(p => p.Name);
            var propsB = a.GetType().GetProperties(flags).OrderBy(p => p.Name);

            var enumeratorB = propsB.GetEnumerator();
            foreach (var curA in propsA)
            {
                enumeratorB.MoveNext();
                var curB = enumeratorB.Current;

                if (curA.Name != curB.Name)
                {
                    return false;
                }

                var valA = curA.GetValue(a);
                var valB = curB.GetValue(b);
                if (!Equals(valA, valB))
                {
                    return false;
                }
            }

            return true;
        }
    }
}