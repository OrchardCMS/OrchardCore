using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Zones;

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
public class ZoneHolding : Shape, IZoneHolding
{
    private readonly Func<ValueTask<IShape>> _zoneFactory;

    public ZoneHolding(Func<ValueTask<IShape>> zoneFactory)
    {
        _zoneFactory = zoneFactory;
    }

    private Zones _zones;

    public Zones Zones => _zones ??= new Zones(_zoneFactory, this);

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
