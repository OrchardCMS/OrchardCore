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
                var appData = System.Environment.GetEnvironmentVariable(ShellOptionConstants.OrchardAppData);
                var configDir = string.IsNullOrWhiteSpace(appData) ? $"{environment.ContentRootPath}/{ShellOptionConstants.DefaultAppDataPath}" : appData;
                LogManager.Configuration.Variables["configDir"] = configDir;
            });
    }
}
