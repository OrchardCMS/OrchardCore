using System.Text;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Logging
{
    /// <summary>
    /// Print the tenant name.
    /// </summary>
    [LayoutRenderer(LayoutRendererName)]
    public class TenantLayoutRenderer : AspNetLayoutRendererBase
    {
        public const string LayoutRendererName = "orchard-tenant-name";

        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var tenantName = ShellScope.Context?.Settings.Name;

            // If there is no ShellContext in the Features then the log is rendered from the Host.
            tenantName ??= HttpContextAccessor.HttpContext.Features.Get<ShellContextFeature>()?.ShellContext.Settings.Name ?? "None";

            builder.Append(tenantName);
        }
    }
}
