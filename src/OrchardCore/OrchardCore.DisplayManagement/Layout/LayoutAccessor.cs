using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Layout;

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
            () => ValueTask.FromResult<IShape>(new ZoneHolding(() => _shapeFactory.CreateAsync("Zone")))) as IZoneHolding;

        if (layout == null)
        {
            // At this point a Layout shape should always exist.
            throw new ApplicationException("Fatal error, a Layout couldn't be created.");
        }

        _layout = Task.FromResult(layout);
        return layout;
    }
}
