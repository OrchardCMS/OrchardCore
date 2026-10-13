using System.Diagnostics;
using System.Diagnostics.Metrics;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Telemetry;

namespace OrchardCore.DisplayManagement.Implementation;

/// <summary>
/// Measures the rendering of shapes, and traces the slow ones.
/// </summary>
internal static class ShapeTelemetry
{
    private static readonly InstrumentAdvice<double> s_durationAdvice = new()
    {
        HistogramBucketBoundaries = [0.0005, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1],
    };

    public static Histogram<double> RenderDuration { get; } = OrchardCoreTelemetry.Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.ShapeRenderDuration,
        unit: "s",
        description: "Duration of rendering a shape, including the shapes it renders.",
        tags: null,
        advice: s_durationAdvice);

    /// <summary>
    /// Gets whether anything listens to the rendering of shapes, so that it's worth measuring.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            return RenderDuration.Enabled || OrchardCoreTelemetry.ActivitySource.HasListeners();
        }
    }

    /// <summary>
    /// Records the duration of the rendering of a shape, and traces it with a span when it was slow.
    /// </summary>
    public static void Record(ShapeMetadata metadata, ShapeBinding binding, long startTimestamp, TimeSpan slowShapeThreshold)
    {
        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
        var tenantName = ShellScope.Context?.Settings.Name;

        if (RenderDuration.Enabled)
        {
            // The shape type only, as its alternates would multiply the number of metric series.
            var tags = new TagList
            {
                { OrchardCoreTelemetryConstants.Attributes.ShapeType, metadata.Type },
            };

            if (tenantName is not null)
            {
                tags.Add(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);
            }

            RenderDuration.Record(elapsed.TotalSeconds, tags);
        }

        if (elapsed < slowShapeThreshold || !OrchardCoreTelemetry.ActivitySource.HasListeners())
        {
            return;
        }

        // The span is only created once the shape is known to be slow, with the time it started at, so that the
        // spans started while rendering faster shapes, such as the ones of database commands, keep a recorded parent.
        using var activity = OrchardCoreTelemetry.ActivitySource.StartActivity(
            $"Render shape {metadata.Type}",
            ActivityKind.Internal,
            parentContext: default,
            startTime: DateTimeOffset.UtcNow - elapsed);

        if (activity is { IsAllDataRequested: true })
        {
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ShapeType, metadata.Type);
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ShapeBinding, binding?.BindingName);
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ShapeBindingSource, binding?.BindingSource);
        }
    }
}
