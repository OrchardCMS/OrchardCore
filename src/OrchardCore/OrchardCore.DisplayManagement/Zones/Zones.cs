using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Zones;

/// <remarks>
/// Returns a ZoneOnDemand object the first time the indexer is invoked.
/// If an item is added to the ZoneOnDemand then the zoneFactory is invoked to create a zone shape and this item is added to the zone.
/// Then the zone shape is assigned in place of the ZoneOnDemand. A ZoneOnDemand returns true when compared to null such that we can
/// do Zones["Foo"] == null to see if anything has been added to a zone, without instantiating a zone when accessing the indexer.
/// </remarks>
public class Zones : Composite
{
    private readonly Func<ValueTask<IShape>> _zoneFactory;
    private readonly ZoneHolding _parent;

    public bool IsNotEmpty(string name) => this[name] is not ZoneOnDemand;

    public Zones(Func<ValueTask<IShape>> zoneFactory, ZoneHolding parent)
    {
        _zoneFactory = zoneFactory;
        _parent = parent;
    }

    public IShape this[string name]
    {
        get
        {
            TryGetMemberImpl(name, out var result);
            return result as IShape;
        }

        set
        {
            TrySetMemberImpl(name, value);
        }
    }

    public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
    {
        return TryGetMemberImpl(binder.Name, out result);
    }

    protected override bool TryGetMemberImpl(string name, out object result)
    {
        if (!_parent.Properties.TryGetValue(name, out result))
        {
            result = new ZoneOnDemand(_zoneFactory, _parent, name);
        }

        return true;
    }

    protected override bool TrySetMemberImpl(string name, object value)
    {
        _parent.Properties[name] = value;
        return true;
    }
}
