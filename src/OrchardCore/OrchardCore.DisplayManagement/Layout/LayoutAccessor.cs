using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Layout;

public class LayoutAccessor : ILayoutAccessor
{
    private readonly IShapeFactory _shapeFactory;
    private readonly object _syncLock = new();

    private Task<IZoneHolding> _layout;

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

        lock (_syncLock)
        {
            return _layout ??= CreateLayoutTask();
        }
    }

    private Task<IZoneHolding> CreateLayoutTask()
    {
        var completionSource = new TaskCompletionSource<IZoneHolding>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = GetLayoutAwaitedAsync(completionSource);

        return completionSource.Task;

        async Task GetLayoutAwaitedAsync(TaskCompletionSource<IZoneHolding> completionSource)
        {
            try
            {
                completionSource.SetResult(await GetLayoutInternalAsync());
            }
            catch (Exception exception)
            {
                lock (_syncLock)
                {
                    if (_layout == completionSource.Task)
                    {
                        _layout = null;
                    }
                }

                completionSource.SetException(exception);
            }
        }
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

        return layout;
    }
}
