using OrchardCore.DisplayManagement.Implementation;
using StackExchange.Profiling;

namespace OrchardCore.MiniProfiler;

public class ShapeStep : IShapeDisplayEvents
{
    private readonly Dictionary<object, IDisposable> _timings = [];

    public Task DisplayedAsync(ShapeDisplayContext context)
    {
        if (_timings.TryGetValue(context, out var timing))
        {
            _timings.Remove(context);
            timing.Dispose();
        }

        return Task.CompletedTask;
    }

    public Task DisplayingAsync(ShapeDisplayContext context)
    {
        var timing = StackExchange.Profiling.MiniProfiler.Current.Step($"Shape: {context.Shape.Metadata.Type}");
        _timings.Add(context, timing);
        return Task.CompletedTask;
    }

    public Task DisplayingFinalizedAsync(ShapeDisplayContext context)
    {
        if (_timings.TryGetValue(context, out var timing))
        {
            _timings.Remove(context);
            timing.Dispose();
        }

        return Task.CompletedTask;
    }
}
