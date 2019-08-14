using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Zones
{
    /// <summary>
    /// Provides the behavior of shapes that have a Zones property.
    /// Examples include Layout and Item
    ///
    /// * Returns a fake parent object for zones
    /// Foo.Zones
    ///
    /// *
    /// Foo.Zones.Alpha :
    /// Foo.Zones["Alpha"]
    /// Foo.Alpha :same
    ///
    /// </summary>
    public class ZoneHolding : Shape
    {
        private readonly Func<Task<IShape>> _zoneFactory;

        public ZoneHolding(Func<Task<IShape>> zoneFactory)
        {
            _zoneFactory = zoneFactory;
        }

        private Zones _zones;
        public Zones Zones
        {
            get
            {
                if (_zones == null)
                {
                    return _zones = new Zones(_zoneFactory, this);
                }

                return _zones;
            }
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            var name = binder.Name;

            if (!base.TryGetMember(binder, out result) || (null == result))
            {
                // substitute nil results with a robot that turns adds a zone on
                // the parent when .Add is invoked
                result = new ZoneOnDemand(_zoneFactory, this, name);
                TrySetMemberImpl(name, result);
            }

            return true;
        }
    }

    /// <remarks>
    /// InterfaceProxyBehavior()
    /// ZonesBehavior(_zoneFactory, self, _layoutShape) => Create ZoneOnDemand if member access
    /// </remarks>
    public class Zones : Composite
    {
        private readonly Func<Task<IShape>> _zoneFactory;
        private readonly object _parent;

        public Zones(Func<Task<IShape>> zoneFactory, object parent)
        {
            _zoneFactory = zoneFactory;
            _parent = parent;
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            return TryGetMemberImpl(binder.Name, out result);
        }

        protected override bool TryGetMemberImpl(string name, out object result)
        {

            var parentMember = ((dynamic)_parent)[name];
            if (parentMember == null)
            {
                result = new ZoneOnDemand(_zoneFactory, _parent, name);
                return true;
            }

            result = parentMember;
            return true;
        }


        protected override bool TrySetMemberImpl(string name, object value)
        {
            ((dynamic)_parent)[name] = value;
            return true;
        }

        public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
        {

            if (indexes.Count() == 1)
            {
                var key = Convert.ToString(indexes.First());

                return TryGetMemberImpl(key, out result);
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(System.Dynamic.SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Count() == 1)
            {
                var key = Convert.ToString(indexes.First());

                return TrySetMemberImpl(key, value);
            }

            return base.TrySetIndex(binder, indexes, value);
        }
    }

    /// <remarks>
    /// InterfaceProxyBehavior()
    /// NilBehavior() => return Nil on GetMember and GetIndex in all cases
    /// ZoneOnDemandBehavior(_zoneFactory, _parent, name)  => when a zone (Shape) is
    /// created, replace itself with the zone so that Layout.ZoneName is no more equal to Nil
    /// </remarks>
    public class ZoneOnDemand : Shape
    {

        private readonly Func<Task<IShape>> _zoneFactory;
        private readonly object _parent;
        private readonly string _potentialZoneName;

        public ZoneOnDemand(Func<Task<IShape>> zoneFactory, object parent, string potentialZoneName)
        {
            _zoneFactory = zoneFactory;
            _parent = parent;
            _potentialZoneName = potentialZoneName;
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            // NilBehavior
            result = Nil.Instance;
            return true;
        }

        public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
        {
            // NilBehavior
            result = Nil.Instance;
            return true;
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;

            // NilBehavior
            if (!args.Any() && name != "ToString")
            {
                result = Nil.Instance;
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override string ToString()
        {
            return String.Empty;
        }

        public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == typeof(string))
            {
                result = null;
            }
            else if (binder.ReturnType.IsValueType)
            {
                result = Activator.CreateInstance(binder.ReturnType);
            }
            else
            {
                result = null;
            }

            return true;
        }

        public static bool operator ==(ZoneOnDemand a, object b)
        {
            // if ZoneOnDemand is compared to null it must return true
            return b == null || ReferenceEquals(b, Nil.Instance);
        }

        public static bool operator !=(ZoneOnDemand a, object b)
        {
            // if ZoneOnDemand is compared to null it must return true
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return true;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_parent != null ? _parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_potentialZoneName != null ? _potentialZoneName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override Shape Add(object item, string position = null)
        {
            if (item == null)
            {
                return (Shape)_parent;
            }

            dynamic parent = _parent;

            dynamic zone = _zoneFactory().GetAwaiter().GetResult();
            zone.Parent = _parent;
            zone.ZoneName = _potentialZoneName;
            parent[_potentialZoneName] = zone;

            if (position == null)
            {
                return zone.Add(item);
            }

            return zone.Add(item, position);
        }

        public async Task<Shape> AddAsync(object item, string position = null)
        {
            if (item == null)
            {
                return (Shape)_parent;
            }

            dynamic parent = _parent;

            dynamic zone = await _zoneFactory();
            zone.Parent = _parent;
            zone.ZoneName = _potentialZoneName;
            parent[_potentialZoneName] = zone;

            if (position == null)
            {
                return zone.Add(item);
            }

            return zone.Add(item, position);
        }
    }
}

