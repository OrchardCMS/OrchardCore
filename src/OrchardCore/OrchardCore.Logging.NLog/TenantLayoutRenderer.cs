using System.Text;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using OrchardCore.Hosting.ShellBuilders;

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
            var context = HttpContextAccessor.HttpContext;

            // If there is no ShellContext in the Features then the log is rendered from the Host
            var tenantName = context.Features.Get<ShellContext>()?.Settings?.Name ?? "None";
            builder.Append(tenantName);
        }
    }
}
