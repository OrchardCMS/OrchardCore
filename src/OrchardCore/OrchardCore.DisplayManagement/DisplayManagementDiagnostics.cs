using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Exposes the <see cref="ActivitySource"/> and <see cref="Meter"/> names used by the display management,
/// to collect shape rendering traces and metrics, for instance with OpenTelemetry or <c>dotnet-counters</c>.
/// </summary>
/// <remarks>
/// When nothing listens to these sources, rendering a shape only checks a flag.
/// </remarks>
public static class DisplayManagementDiagnostics
{
    /// <summary>
    /// The name of the <see cref="ActivitySource"/> that creates an activity for each rendered shape.
    /// </summary>
    public const string ActivitySourceName = "OrchardCore.DisplayManagement";

    /// <summary>
    /// The name of the <see cref="Meter"/> that records shape rendering metrics.
    /// </summary>
    public const string MeterName = "OrchardCore.DisplayManagement";

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    internal static readonly Meter Meter = new(MeterName);

    /// <summary>
    /// Counts the rendered shapes.
    /// </summary>
    internal static readonly Counter<long> ShapesRendered = Meter.CreateCounter<long>(
        "orchardcore.display.shapes_rendered",
        unit: "{shape}",
        description: "Number of shapes rendered.");

    /// <summary>
    /// Records the duration of shape renderings, including the rendering of their child shapes.
    /// </summary>
    /// <remarks>
    /// Most shapes render in microseconds, so the buckets start at 10 microseconds, while the default buckets
    /// of OpenTelemetry, meant for milliseconds, would put all of them in the first bucket.
    /// </remarks>
    internal static readonly Histogram<double> ShapeRenderDuration = Meter.CreateHistogram(
        "orchardcore.display.shape.duration",
        unit: "s",
        description: "Duration of shape renderings, including their child shapes.",
        tags: null,
        advice: new InstrumentAdvice<double>
        {
            HistogramBucketBoundaries = [0.00001, 0.00002, 0.00005, 0.0001, 0.0002, 0.0005, 0.001, 0.002, 0.005, 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5],
        });
}
