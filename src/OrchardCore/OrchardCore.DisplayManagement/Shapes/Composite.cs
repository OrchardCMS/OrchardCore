using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardCore.DisplayManagement.Shapes
{
    public class Composite : DynamicObject
    {
        protected readonly Dictionary<object, object> _props = new Dictionary<object, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMemberImpl(binder.Name, out result);
        }

        protected virtual bool TryGetMemberImpl(string name, out object result)
        {
            if (_props.TryGetValue(name, out result))
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
            _props[name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0)
            {
                return TryGetMemberImpl(binder.Name, out result);
            }

            // method call with one argument will assign the property
            if (args.Length == 1)
            {
                result = this;
                return TrySetMemberImpl(binder.Name, args.First());
            }

            if (!base.TryInvokeMember(binder, args, out result))
            {
                if (binder.Name == "ToString")
                {
                    result = string.Empty;
                    return true;
                }

                return false;
            }

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                return base.TryGetIndex(binder, indexes, out result);
            }

            var index = indexes.First();

            if (_props.TryGetValue(index, out result))
            {
                return true;
            }

            // try to access an existing member
            var strinIndex = index as string;

            if (strinIndex != null && TryGetMemberImpl(strinIndex, out result))
            {
                return true;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                return base.TrySetIndex(binder, indexes, value);
            }

            var index = indexes[0];

            // try to access an existing member
            var strinIndex = index as string;

            if (strinIndex != null && TrySetMemberImpl(strinIndex, value))
            {
                return true;
            }

            _props[indexes[0]] = value;
            return true;
        }

        public IDictionary<object, object> Properties
        {
            get { return _props; }
        }

        public static bool operator ==(Composite a, Nil b)
        {
            return null == a;
        }

        public static bool operator !=(Composite a, Nil b)
        {
            return !(a == b);
        }

        protected bool Equals(Composite other)
        {
            return Equals(_props, other._props);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Composite)obj);
        }

        public override int GetHashCode()
        {
            return (_props != null ? _props.GetHashCode() : 0);
        }
    }

    public class Nil : DynamicObject
    {
        static readonly Nil Singleton = new Nil();
        public static Nil Instance { get { return Singleton; } }

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
                    result = ReferenceEquals(arg, Nil.Instance) || (object)arg == null;
                    return true;
                case ExpressionType.NotEqual:
                    result = !ReferenceEquals(arg, Nil.Instance) && (object)arg != null;
                    return true;
            }

            return base.TryBinaryOperation(binder, arg, out result);
        }

        public static bool operator ==(Nil a, Nil b)
        {
            return true;
        }

        public static bool operator !=(Nil a, Nil b)
        {
            return false;
        }

        public static bool operator ==(Nil a, object b)
        {
            return ReferenceEquals(a, b) || (object)b == null;
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
            return string.Empty;
        }
    }
}