using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.LayoutRenderers;
using NLog.Web;

namespace Orchard.Logging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddNLogWeb(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
            loggerFactory.AddNLog();
            app.AddNLogWeb();

            return app;
        }
    }
}
