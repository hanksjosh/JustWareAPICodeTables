using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JustWareApiCodeTables
{
    public class ReflectionInfo
    {
        private readonly Type _type;
        private IEnumerable<MemberInfo> _members;

        private IEnumerable<PropertyInfo> _properties;
        private IEnumerable<FieldInfo> _fields;
        private IEnumerable<MethodInfo> _methods;
        private IEnumerable<ConstructorInfo> _constructors;
        private IEnumerable<EventInfo> _events;
        private IEnumerable<Attribute> _attributes;

        public ReflectionInfo(Type type)
        {
            _type = type;
        }

        public Type Type
        {
            get { return _type; }
        }

        public IEnumerable<Type> TypeHierarchy
        {
            get
            {
                Type t = _type;
                while (t != null)
                {
                    yield return t;
                    t = t.BaseType;
                }
            }
        }

        public IEnumerable<MemberInfo> Members
        {
            get
            {
                if (_members == null)
                {
                    _members = TypeHierarchy.SelectMany(t => t.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();
                }

                return _members;
            }
        }

        public IEnumerable<PropertyInfo> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = Members.OfType<PropertyInfo>();
                }

                return _properties;
            }
        }

        public IEnumerable<FieldInfo> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = Members.OfType<FieldInfo>();
                }

                return _fields;
            }
        }

        public IEnumerable<MethodInfo> Methods
        {
            get
            {
                if (_methods == null)
                {
                    _methods = Members.OfType<MethodInfo>();
                }

                return _methods;
            }
        }

        public IEnumerable<ConstructorInfo> Constructors
        {
            get
            {
                if (_constructors == null)
                {
                    _constructors = Members.OfType<ConstructorInfo>();
                }

                return _constructors;
            }
        }

        public IEnumerable<EventInfo> Events
        {
            get
            {
                if (_events == null)
                {
                    _events = Members.OfType<EventInfo>();
                }

                return _events;
            }
        }

        public IEnumerable<Attribute> Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    _attributes = _type.GetCustomAttributes(true).OfType<Attribute>();
                }

                return _attributes;
            }
        }
    }
}