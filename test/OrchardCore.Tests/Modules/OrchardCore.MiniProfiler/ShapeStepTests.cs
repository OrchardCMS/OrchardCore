using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.MiniProfiler;
using StackExchange.Profiling.Internal;

namespace OrchardCore.Tests.Modules.OrchardCore.MiniProfiler;

public class ShapeStepTests
{
    [Fact]
    public async Task DeeplyNestedShapes_KeepTheProfileSerializable()
    {
        var profiler = StackExchange.Profiling.MiniProfiler.StartNew("Nested shapes");
        var shapeStep = new ShapeStep();
        var contexts = new List<ShapeDisplayContext>();

        // A full admin page nests shapes deeper than System.Text.Json's default limit of 64 allows for timings.
        for (var i = 0; i < 40; i++)
        {
            var shape = new Shape();
            shape.Metadata.Type = $"Shape{i}";

            var context = new ShapeDisplayContext { Shape = shape };
            contexts.Add(context);

            await shapeStep.DisplayingAsync(context);
        }

        for (var i = contexts.Count - 1; i >= 0; i--)
        {
            await shapeStep.DisplayedAsync(contexts[i]);
        }

        await profiler.StopAsync();

        var json = profiler.ToJson();

        Assert.Contains("Shape: Shape0", json);
        Assert.DoesNotContain("Shape: Shape39", json);
    }
}
