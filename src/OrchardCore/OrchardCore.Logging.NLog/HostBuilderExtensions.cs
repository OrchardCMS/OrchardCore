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
                var appData = System.Environment.GetEnvironmentVariable("ORCHARD_APP_DATA");
                var configDir = string.IsNullOrWhiteSpace(appData) ? $"{environment.ContentRootPath}/App_Data" : appData;
                LogManager.Configuration.Variables["configDir"] = configDir;
            });
    }
}
