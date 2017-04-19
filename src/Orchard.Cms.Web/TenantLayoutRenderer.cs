using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using NLog;
using Orchard.Environment.Shell;

namespace Orchard.Cms.Web
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
