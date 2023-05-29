using System.IO;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web;

namespace OrchardCore.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNLogHost(this IHostBuilder builder)
    {
        LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
        builder.UseNLog();
        builder.ConfigureAppConfiguration((context, _) =>
        {
            var environment = context.HostingEnvironment;
            environment.ConfigureNLog($"{environment.ContentRootPath}{Path.DirectorySeparatorChar}NLog.config");
            LogManager.Configuration.Variables["configDir"] = environment.ContentRootPath;
        });

        return builder;
    }
}
