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
/// Foo.Alpha :same.
///
/// </summary>
public class ZoneHolding : Shape, IZoneHolding
{
    private readonly Func<ValueTask<IShape>> _zoneFactory;
    private Zones _zones;

    public ZoneHolding(Func<ValueTask<IShape>> zoneFactory)
    {
        _zoneFactory = zoneFactory;
    }

    protected ZoneHolding()
    {
    }

    public Zones Zones => _zones ??= new Zones(this);

    public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
    {
        var name = binder.Name;

        if (!base.TryGetMember(binder, out result) || (null == result))
        {
            // substitute nil results with a robot that turns adds a zone on
            // the parent when .Add is invoked
            result = new ZoneOnDemand(this, name);
            TrySetMemberImpl(name, result);
        }

        return true;
    }

    internal virtual ValueTask<IShape> CreateZoneAsync()
    {
        return _zoneFactory != null ? _zoneFactory() : ValueTask.FromResult<IShape>(null);
    }
}

public sealed class ZoneHolding<T> : ZoneHolding
{
    private readonly Func<T, ValueTask<IShape>> _zoneFactoryWithState;
    private readonly T _state;

    public ZoneHolding(Func<T, ValueTask<IShape>> zoneFactory, T state)
    {
        _zoneFactoryWithState = zoneFactory;
        _state = state;
    }

    internal override ValueTask<IShape> CreateZoneAsync()
    {
        return _zoneFactoryWithState != null ? _zoneFactoryWithState(_state) : base.CreateZoneAsync();
    }
}
