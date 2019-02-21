using System.Text;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Logging
{
    /// <summary>
    /// Print the tenant name
    /// </summary>
    [LayoutRenderer(LayoutRendererName)]
    public class TenantLayoutRenderer : AspNetLayoutRendererBase
    {
        public const string LayoutRendererName = "orchard-tenant-name";

        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            // If there is no ShellScope then the log is rendered from the Host
            var tenantName = ShellScope.Context?.Settings.Name ?? "None";
            builder.Append(tenantName);
        }
    }
}
