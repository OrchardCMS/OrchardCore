using System.Text;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using Orchard.Environment.Shell;

namespace Orchard.Logging
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

            builder.Append(context.Features.Get<ShellSettings>().Name);
        }
    }
}
