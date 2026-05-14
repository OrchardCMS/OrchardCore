using System.Text;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Logging;

/// <summary>
/// Print the tenant name.
/// </summary>
[LayoutRenderer(LayoutRendererName)]
public class TenantLayoutRenderer : AspNetLayoutRendererBase
{
    public const string LayoutRendererName = "orchard-tenant-name";

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        // If there is no ShellContext in the Features then the log is rendered from the Host.
        var tenantName = HttpContextAccessor?.HttpContext?.Features.Get<ShellContextFeature>()
            ?.ShellContext.Settings.Name;

        tenantName ??= ShellScope.Context?.Settings.Name;

        builder.Append(tenantName ?? "None");
    }
}
