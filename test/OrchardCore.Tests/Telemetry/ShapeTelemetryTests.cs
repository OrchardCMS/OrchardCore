using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DynamicCache;
using OrchardCore.DynamicCache.EventHandlers;
using OrchardCore.Environment.Cache;
using OrchardCore.Telemetry;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Telemetry;

public class ShapeTelemetryTests
{
    [Fact]
    public void RecordSlow_SlowerThanThreshold_TracesShapeFromItsStart()
    {
        var shapeType = $"SlowShape{Guid.NewGuid():n}";
        var activities = new ConcurrentQueue<Activity>();
        using var listener = ListenToOrchardCore(activities);

        var metadata = new ShapeMetadata { Type = shapeType };
        var binding = new ShapeBinding { BindingName = shapeType, BindingSource = "Views/SlowShape.cshtml" };
        ShapeTelemetry.RecordSlow(metadata, binding, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(100));

        var activity = Assert.Single(activities, activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ShapeType) as string == shapeType);
        Assert.Equal($"Render shape {shapeType}", activity.DisplayName);
        Assert.Equal("Views/SlowShape.cshtml", activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ShapeBinding));
        Assert.True(activity.Duration >= TimeSpan.FromMilliseconds(190), $"The span lasted {activity.Duration}.");
    }

    [Fact]
    public void RecordSlow_FasterThanThreshold_DoesNotTraceShape()
    {
        var shapeType = $"FastShape{Guid.NewGuid():n}";
        var activities = new ConcurrentQueue<Activity>();
        using var listener = ListenToOrchardCore(activities);

        ShapeTelemetry.RecordSlow(new ShapeMetadata { Type = shapeType }, binding: null, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1));

        Assert.DoesNotContain(activities, activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ShapeType) as string == shapeType);
    }

    [Theory]
    [InlineData(true, "hit")]
    [InlineData(false, "miss")]
    public async Task DynamicCacheShapeDisplayEvents_CachedShape_CountsLookup(bool found, string result)
    {
        var shapeType = $"CachedShape{Guid.NewGuid():n}";
        var lookups = new ConcurrentQueue<(string ShapeType, string Result)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == OrchardCoreTelemetryConstants.MeterName &&
                    instrument.Name == OrchardCoreTelemetryConstants.Instruments.ShapeCacheLookups)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        meterListener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            var values = ToDictionary(tags);
            lookups.Enqueue((values.GetValueOrDefault(OrchardCoreTelemetryConstants.Attributes.ShapeType) as string,
                values.GetValueOrDefault(OrchardCoreTelemetryConstants.Attributes.ShapeCacheResult) as string));
        });
        meterListener.Start();

        var cacheService = new Mock<IDynamicCacheService>();
        cacheService
            .Setup(service => service.GetCachedValueAsync(It.IsAny<CacheContext>()))
            .ReturnsAsync(found ? "<p>Cached</p>" : null);

        var events = new DynamicCacheShapeDisplayEvents(
            cacheService.Object,
            Mock.Of<ICacheScopeManager>(),
            HtmlEncoder.Default,
            Options.Create(new CacheOptions()));

        var shape = new Shape();
        shape.Metadata.Type = shapeType;
        shape.Metadata.Cache("cached-shape");

        await events.DisplayingAsync(new ShapeDisplayContext { Shape = shape });

        Assert.Contains((shapeType, result), lookups);
    }

    private static Dictionary<string, object> ToDictionary(ReadOnlySpan<KeyValuePair<string, object>> tags)
    {
        var values = new Dictionary<string, object>();
        foreach (var tag in tags)
        {
            values[tag.Key] = tag.Value;
        }

        return values;
    }

    private static ActivityListener ListenToOrchardCore(ConcurrentQueue<Activity> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OrchardCoreTelemetryConstants.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };
        ActivitySource.AddActivityListener(listener);

        return listener;
    }
}
