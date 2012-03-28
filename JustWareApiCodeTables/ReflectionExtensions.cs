using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JustWareApiCodeTables
{
    [DebuggerStepThrough]
    public static class ReflectionExtensions
    {
        // http://blogs.msdn.com/b/davidebb/archive/2010/01/18/use-c-4-0-dynamic-to-drastically-simplify-your-private-reflection-code.aspx
        [DebuggerStepThrough]
        class PrivateReflectionDynamicObject : DynamicObject
        {
            private static readonly IDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();

            // Simple abstraction to make field and property access consistent
            interface IProperty
            {
                string Name { get; }
                object GetValue(object obj, object[] index);
                void SetValue(object obj, object val, object[] index);
            }

            // IProperty implementation over a PropertyInfo
            class Property : IProperty
            {
                internal PropertyInfo PropertyInfo { get; set; }

                string IProperty.Name
                {
                    get
                    {
                        return PropertyInfo.Name;
                    }
                }

                object IProperty.GetValue(object obj, object[] index)
                {
                    return PropertyInfo.GetValue(obj, index);
                }

                void IProperty.SetValue(object obj, object val, object[] index)
                {
                    if (val is PrivateReflectionDynamicObject && PropertyInfo.PropertyType.IsAssignableFrom(((PrivateReflectionDynamicObject)val).RealObject.GetType()))
                    {
                        PropertyInfo.SetValue(obj, ((PrivateReflectionDynamicObject)val).RealObject, index);
                    }
                    else
                    {
                        PropertyInfo.SetValue(obj, val, index);
                    }
                }
            }

            // IProperty implementation over a FieldInfo
            class Field : IProperty
            {
                internal FieldInfo FieldInfo { get; set; }

                string IProperty.Name
                {
                    get
                    {
                        return FieldInfo.Name;
                    }
                }


                object IProperty.GetValue(object obj, object[] index)
                {
                    return FieldInfo.GetValue(obj);
                }

                void IProperty.SetValue(object obj, object val, object[] index)
                {
                    if (val is PrivateReflectionDynamicObject && FieldInfo.FieldType.IsAssignableFrom(((PrivateReflectionDynamicObject)val).RealObject.GetType()))
                    {
                        FieldInfo.SetValue(obj, ((PrivateReflectionDynamicObject)val).RealObject);
                    }
                    else
                    {
                        FieldInfo.SetValue(obj, val);
                    }

                }
            }

            class Method : IProperty
            {
                internal MethodInfo MethodInfo { get; set; }

                string IProperty.Name
                {
                    get
                    {
                        return MethodInfo.Name;
                    }
                }

                object IProperty.GetValue(object obj, object[] index)
                {
                    return Delegate.CreateDelegate(Expression.GetDelegateType(MethodInfo.GetParameters().Select(p => p.ParameterType).Concat(new Type[] { MethodInfo.ReturnType }).ToArray()), obj, MethodInfo);
                }

                void IProperty.SetValue(object obj, object val, object[] index)
                {
                    throw new NotSupportedException("Unable to set a method to something.");
                }
            }


            private object RealObject { get; set; }
            private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            internal static object WrapObjectIfNeeded(object o)
            {
                // Don't wrap primitive types, which don't have many interesting internal APIs
                if (o == null || o.GetType().IsPrimitive || o is string || o is Delegate || o is Enum || o is decimal || o is DateTime)
                    return o;

                return new PrivateReflectionDynamicObject() { RealObject = o };
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                IProperty prop = GetProperty(binder.Name);

                // Get the property value
                result = prop.GetValue(RealObject, index: null);

                // Wrap the sub object if necessary. This allows nested anonymous objects to work.
                result = WrapObjectIfNeeded(result);

                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                IProperty prop = GetProperty(binder.Name);

                // Set the property value
                prop.SetValue(RealObject, value, index: null);

                return true;
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                // The indexed property is always named "Item" in C#
                IProperty prop = GetIndexProperty(indexes);
                result = prop.GetValue(RealObject, indexes);

                // Wrap the sub object if necessary. This allows nested anonymous objects to work.
                result = WrapObjectIfNeeded(result);

                return true;
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                // The indexed property is always named "Item" in C#
                IProperty prop = GetIndexProperty(indexes);
                prop.SetValue(RealObject, value, indexes);
                return true;
            }

            // Called when a method is called
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

                // Wrap the sub object if necessary. This allows nested anonymous objects to work.
                result = WrapObjectIfNeeded(result);

                return true;
            }

            public override bool TryConvert(ConvertBinder binder, out object result)
            {
                if (binder.Type.IsAssignableFrom(RealObject.GetType()))
                {
                    result = RealObject;
                }
                else
                {
                    try
                    {
                        result = RealObject.ChangeType(binder.Type);
                    }
                    catch
                    {
                        result = null;
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return RealObject.ToString();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;

                if (obj is PrivateReflectionDynamicObject)
                {
                    return RealObject.Equals(((PrivateReflectionDynamicObject)obj).RealObject);
                }
                return RealObject.Equals(obj);
            }

            public override int GetHashCode()
            {
                return RealObject.GetHashCode();
            }

            private IProperty GetIndexProperty(object[] indexes)
            {
                // The index property is always named "Item" in C#
                var prop = RealObject.GetType().GetProperty("Item", bindingFlags, null, null, indexes.Select(i => i.GetType()).ToArray(), null);
                return new Property { PropertyInfo = prop };
            }

            private IProperty GetProperty(string propertyName)
            {
                Type type = RealObject.GetType();

                // Get the list of properties and fields for this type
                IProperty property = GetPropertyInternal(propertyName, type);
                if (property != null) return property;

                // The property doesn't exist on this class, check parent classes
                while (type.BaseType != null && type.BaseType != typeof(object))
                {
                    property = GetPropertyInternal(propertyName, type.BaseType);
                    if (property != null) return property;

                    type = type.BaseType;
                }

                IDictionary<string, IProperty> typeProperties = GetTypeProperties(RealObject.GetType());

                // Get a list of supported properties and fields and show them as part of the exception message
                // For fields, skip the auto property backing fields (which name start with <)
                var propNames = typeProperties.Keys.Where(name => name[0] != '<').OrderBy(name => name);
                throw new ArgumentException(
                    String.Format(
                    "The property {0} doesn't exist on type {1}. Supported properties are: {2}",
                    propertyName, RealObject.GetType(), String.Join(", ", propNames)));
            }

            private IProperty GetPropertyInternal(string propertyName, Type type)
            {
                IDictionary<string, IProperty> typeProperties = GetTypeProperties(type);

                // Look for the one we want
                IProperty property;
                if (typeProperties.TryGetValue(propertyName, out property))
                {
                    return property;
                }

                return null;
            }

            private static IDictionary<string, IProperty> GetTypeProperties(Type type)
            {
                // First, check if we already have it cached
                IDictionary<string, IProperty> typeProperties;
                if (_propertiesOnType.TryGetValue(type, out typeProperties))
                {
                    return typeProperties;
                }

                // Not cache, so we need to build it

                typeProperties = new ConcurrentDictionary<string, IProperty>();

                // First, add all the properties
                foreach (PropertyInfo prop in type.GetProperties(bindingFlags).Where(p => p.DeclaringType == type))
                {
                    typeProperties[prop.Name] = new Property() { PropertyInfo = prop };
                }

                // Now, add all the fields
                foreach (FieldInfo field in type.GetFields(bindingFlags).Where(p => p.DeclaringType == type))
                {
                    typeProperties[field.Name] = new Field() { FieldInfo = field };
                }

                // add the methods for use as delegates
                foreach (MethodInfo meth in type.GetMethods(bindingFlags).Where(p => p.DeclaringType == type))
                {
                    typeProperties[meth.Name] = new Method() { MethodInfo = meth };
                }

                // Finally, recurse on the base class to add its fields
                if (type.BaseType != null)
                {
                    foreach (IProperty prop in GetTypeProperties(type.BaseType).Values)
                    {
                        typeProperties[prop.Name] = prop;
                    }
                }

                // Cache it for next time
                _propertiesOnType[type] = typeProperties;

                return typeProperties;
            }

            private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
            {
                try
                {
                    if (args != null)
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] is PrivateReflectionDynamicObject) args[i] = ((PrivateReflectionDynamicObject)args[i]).RealObject;
                        }
                    }

                    // Try to incoke the method
                    return type.InvokeMember(
                        name,
                        BindingFlags.InvokeMethod | bindingFlags,
                        null,
                        target,
                        args);
                }
                catch (MissingMethodException)
                {
                    // If we couldn't find the method, try on the base class
                    if (type.BaseType != null)
                    {
                        return InvokeMemberOnType(type.BaseType, target, name, args);
                    }

                    throw;
                }
            }
        }

        public static object ChangeType(this object value, Type destinationType)
        {
            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;

                var nullableConverter = new System.ComponentModel.NullableConverter(destinationType);
                if (nullableConverter.CanConvertFrom(value.GetType()))
                {
                    return nullableConverter.ConvertFrom(value);
                }
                destinationType = nullableConverter.UnderlyingType;
            }

            return Convert.ChangeType(value, destinationType);
        }


        public static dynamic AsDynamic(this object o) { return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o); }
    }
}