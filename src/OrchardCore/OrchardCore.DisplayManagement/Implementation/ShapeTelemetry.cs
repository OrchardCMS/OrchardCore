using System.Diagnostics;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Telemetry;

namespace OrchardCore.DisplayManagement.Implementation;

/// <summary>
/// Traces the slow renderings of shapes.
/// </summary>
internal static class ShapeTelemetry
{
    /// <summary>
    /// Gets whether anything listens to the traces, so that it's worth measuring.
    /// </summary>
    public static bool IsEnabled => OrchardCoreTelemetry.ActivitySource.HasListeners();

    /// <summary>
    /// Traces the rendering of a shape with a span when it was slow.
    /// </summary>
    public static void RecordSlow(ShapeMetadata metadata, ShapeBinding binding, TimeSpan elapsed, TimeSpan slowShapeThreshold)
    {
        if (elapsed < slowShapeThreshold || !OrchardCoreTelemetry.ActivitySource.HasListeners())
        {
            return;
        }

        // The span is only created once the shape is known to be slow, with the time it started at, so that the
        // spans started while rendering faster shapes keep a recorded parent.
        using var activity = OrchardCoreTelemetry.ActivitySource.StartActivity(
            $"Render shape {metadata.Type}",
            ActivityKind.Internal,
            parentContext: default,
            startTime: DateTimeOffset.UtcNow - elapsed);

        if (activity is { IsAllDataRequested: true })
        {
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ShapeType, metadata.Type);
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, ShellScope.Context?.Settings.Name);
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ShapeBinding, binding?.BindingSource);
        }
    }
}
