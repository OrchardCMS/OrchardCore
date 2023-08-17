using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardCore.DisplayManagement.Shapes
{
    public class Composite : DynamicObject
    {
        protected readonly Dictionary<string, object> _properties = new();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMemberImpl(binder.Name, out result);
        }

        protected virtual bool TryGetMemberImpl(string name, out object result)
        {
            if (_properties.TryGetValue(name, out result))
            {
                return true;
            }

            result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMemberImpl(binder.Name, value);
        }

        protected virtual bool TrySetMemberImpl(string name, object value)
        {
            _properties[name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0)
            {
                return TryGetMemberImpl(binder.Name, out result);
            }

            // Method call with one argument will assign the property.
            if (args.Length == 1)
            {
                result = this;
                return TrySetMemberImpl(binder.Name, args.First());
            }

            if (!base.TryInvokeMember(binder, args, out result))
            {
                if (binder.Name == "ToString")
                {
                    result = String.Empty;
                    return true;
                }

                return false;
            }

            return true;
        }

        public virtual bool TryGetIndexImpl(string name, out object result)
        {
            if (name != null && TryGetMemberImpl(name, out result))
            {
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1)
            {
                var stringIndex = indexes[0] as string;

                return TryGetIndexImpl(stringIndex, out result);
            }
            // Returning false results in a RuntimeBinderException if the index supplied is not an existing string property name.
            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1)
            {
                // try to access an existing member.
                var stringIndex = indexes[0] as string;

                if (stringIndex != null && TrySetMemberImpl(stringIndex, value))
                {
                    return true;
                }
                else
                {
                    // Returning false results in a RuntimeBinderException if the index supplied is not an existing string property name.
                    return false;
                }
            }
            // Returning false results in a RuntimeBinderException if the index supplied is not an existing string property name.
            return false;
        }

        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }

        public static bool operator ==(Composite a, Nil _)
        {
            return null == a;
        }

        public static bool operator !=(Composite a, Nil b)
        {
            return !(a == b);
        }

        protected bool Equals(Composite other)
        {
            return Equals(_properties, other._properties);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Composite)obj);
        }

        public override int GetHashCode()
        {
            return (_properties != null ? _properties.GetHashCode() : 0);
        }
    }

    public class Nil : DynamicObject
    {
        private static readonly Nil _singleton = new();
        public static Nil Instance { get { return _singleton; } }

        private Nil()
        {
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Instance;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = Instance;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Nil.Instance;
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            switch (binder.Operation)
            {
                case ExpressionType.Equal:
                    result = ReferenceEquals(arg, Nil.Instance) || arg == null;
                    return true;
                case ExpressionType.NotEqual:
                    result = !ReferenceEquals(arg, Nil.Instance) && arg != null;
                    return true;
            }

            return base.TryBinaryOperation(binder, arg, out result);
        }

        public static bool operator ==(Nil _1, Nil _2)
        {
            return true;
        }

        public static bool operator !=(Nil _1, Nil _2)
        {
            return false;
        }

        public static bool operator ==(Nil a, object b)
        {
            return ReferenceEquals(a, b) || b == null;
        }

        public static bool operator !=(Nil a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return true;
            }

            return ReferenceEquals(obj, Nil.Instance);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            return true;
        }

        public override string ToString()
        {
            return String.Empty;
        }
    }
}
