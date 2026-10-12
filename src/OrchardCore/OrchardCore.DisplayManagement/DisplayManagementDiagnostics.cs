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
    internal static readonly Histogram<double> ShapeRenderDuration = Meter.CreateHistogram<double>(
        "orchardcore.display.shape.duration",
        unit: "s",
        description: "Duration of shape renderings, including their child shapes.");
}
