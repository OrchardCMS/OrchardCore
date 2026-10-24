using OpenTelemetry;
using OpenTelemetry.Logs;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Telemetry;

/// <summary>
/// Adds the name of the tenant to the log records written in the scope of this tenant.
/// </summary>
internal sealed class TenantLogRecordProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        var tenantName = ShellScope.Context?.Settings.Name;
        if (tenantName is null)
        {
            return;
        }

        var attributes = data.Attributes;
        var count = attributes?.Count ?? 0;

        var enriched = new KeyValuePair<string, object>[count + 1];
        for (var i = 0; i < count; i++)
        {
            enriched[i] = attributes[i];
        }

        enriched[count] = new(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);

        data.Attributes = enriched;
    }
}
