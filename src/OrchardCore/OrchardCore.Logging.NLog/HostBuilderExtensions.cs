using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Web;

namespace OrchardCore.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNLogHost(this IHostBuilder builder)
    {
        LogManager.Setup().SetupExtensions(ext =>
            ext.RegisterLayoutRenderer<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName));

        return builder
            .UseNLog()
            .ConfigureAppConfiguration((context, _) =>
            {
                var environment = context.HostingEnvironment;
                LogManager.Configuration.Variables["configDir"] = environment.ContentRootPath;
            });
    }
}
