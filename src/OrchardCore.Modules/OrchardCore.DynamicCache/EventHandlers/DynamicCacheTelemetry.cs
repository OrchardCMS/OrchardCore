using System.Diagnostics;
using System.Diagnostics.Metrics;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Telemetry;

namespace OrchardCore.DynamicCache.EventHandlers;

/// <summary>
/// Counts the lookups of the content of cached shapes, by result.
/// </summary>
internal static class DynamicCacheTelemetry
{
    private const string Hit = "hit";
    private const string Miss = "miss";

    public static Counter<long> Lookups { get; } = OrchardCoreTelemetry.Meter.CreateCounter<long>(
        OrchardCoreTelemetryConstants.Instruments.ShapeCacheLookups,
        unit: "{lookup}",
        description: "Number of lookups of the content of a cached shape, by result.");

    public static void RecordLookup(string shapeType, bool found)
    {
        if (!Lookups.Enabled)
        {
            return;
        }

        var tags = new TagList
        {
            { OrchardCoreTelemetryConstants.Attributes.ShapeType, shapeType },
            { OrchardCoreTelemetryConstants.Attributes.ShapeCacheResult, found ? Hit : Miss },
        };

        var tenantName = ShellScope.Context?.Settings.Name;
        if (tenantName is not null)
        {
            tags.Add(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);
        }

        Lookups.Add(1, tags);
    }
}
