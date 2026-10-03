using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Layout;

/// <summary>
/// Provides access to the current layout shape for the active scope.
/// </summary>
/// <remarks>
/// This implementation is scoped per request and intentionally not thread-safe.
/// It is used by Orchard Core's sequential display pipeline and must not be registered as a singleton.
/// </remarks>
public class LayoutAccessor : ILayoutAccessor
{
    private Task<IZoneHolding> _layout;
    private readonly IShapeFactory _shapeFactory;

    public LayoutAccessor(IShapeFactory shapeFactory)
    {
        _shapeFactory = shapeFactory;
    }

    public Task<IZoneHolding> GetLayoutAsync()
    {
        if (_layout != null)
        {
            return _layout;
        }

        return GetLayoutInternalAsync();
    }

    private async Task<IZoneHolding> GetLayoutInternalAsync()
    {
        // Create a shape whose properties are dynamically created as Zone shapes.
        var layout = await _shapeFactory.CreateAsync(
            "Layout",
            static (shapeFactory) =>
                ValueTask.FromResult<IShape>(
                    new ZoneHolding<IShapeFactory>(
                        static (factory) => factory.CreateAsync("Zone"),
                        shapeFactory)),
            _shapeFactory) as IZoneHolding;

        if (layout == null)
        {
            // At this point a Layout shape should always exist.
            throw new ApplicationException("Fatal error, a Layout couldn't be created.");
        }

        _layout = Task.FromResult(layout);
        return layout;
    }
}
