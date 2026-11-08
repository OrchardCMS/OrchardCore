using System.Diagnostics;
using OpenTelemetry;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Telemetry;

/// <summary>
/// Tags the spans started in the scope of a tenant with the name of this tenant.
/// </summary>
internal sealed class TenantActivityProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity data)
    {
        var tenantName = ShellScope.Context?.Settings.Name;
        if (tenantName is not null && data.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) is null)
        {
            data.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);
        }
    }
}
