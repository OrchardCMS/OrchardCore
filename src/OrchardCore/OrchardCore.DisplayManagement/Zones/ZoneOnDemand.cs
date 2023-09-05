using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Zones
{
    /// <remarks>
    /// InterfaceProxyBehavior().
    /// NilBehavior() => return Nil on GetMember and GetIndex in all cases.
    /// ZoneOnDemandBehavior(_zoneFactory, _parent, name) => when a zone (Shape) is
    /// created, replace itself with the zone so that Layout.ZoneName is no more equal to Nil.
    /// </remarks>
    public class ZoneOnDemand : Shape
    {
        private readonly Func<ValueTask<IShape>> _zoneFactory;
        private readonly ZoneHolding _parent;
        private readonly string _potentialZoneName;
        private IShape _zone;

        public ZoneOnDemand(Func<ValueTask<IShape>> zoneFactory, ZoneHolding parent, string potentialZoneName)
        {
            _zoneFactory = zoneFactory;
            _parent = parent;
            _potentialZoneName = potentialZoneName;
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            // NilBehavior.
            result = Nil.Instance;
            return true;
        }

        public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
        {
            // NilBehavior.
            result = Nil.Instance;
            return true;
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;

            // NilBehavior.
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

        public static bool operator ==(ZoneOnDemand _, object b)
        {
            // If ZoneOnDemand is compared to null it must return true.
            return b == null || ReferenceEquals(b, Nil.Instance);
        }

        public static bool operator !=(ZoneOnDemand a, object b)
        {
            // If ZoneOnDemand is compared to null it must return true.
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
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
            return HashCode.Combine(_parent, _potentialZoneName);
        }

        public override async ValueTask<IShape> AddAsync(object item, string position)
        {
            if (item == null)
            {
                if (_zone != null)
                {
                    return _zone;
                }

                return this;
            }

            if (_zone == null)
            {
                _zone = await _zoneFactory();
                _zone.Properties["Parent"] = _parent;
                _zone.Properties["ZoneName"] = _potentialZoneName;
                _parent.Properties[_potentialZoneName] = _zone;
            }

            return _zone = await _zone.AddAsync(item, position);
        }
    }
}
