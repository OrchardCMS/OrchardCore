using OrchardCore.DisplayManagement.Implementation;
using StackExchange.Profiling;

namespace OrchardCore.MiniProfiler;

public class ShapeStep : IShapeDisplayEvents
{
    // MiniProfiler serializes its results with System.Text.Json, which throws past 64 levels of nesting, and every
    // timing adds two. Shapes deeper than this are not timed on their own; their time stays in the closest timed parent.
    private const int MaxTimingDepth = 20;

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
        var profiler = StackExchange.Profiling.MiniProfiler.Current;

        if (profiler is null || profiler.Head?.Depth >= MaxTimingDepth)
        {
            return Task.CompletedTask;
        }

        _timings.Add(context, profiler.Step($"Shape: {context.Shape.Metadata.Type}"));

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
