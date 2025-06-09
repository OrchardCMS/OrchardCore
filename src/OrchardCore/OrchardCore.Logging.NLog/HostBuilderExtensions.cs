using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
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
            var configDir = string.IsNullOrWhiteSpace(appData) 
                ? Path.Combine(environment.ContentRootPath, ShellOptionConstants.DefaultAppDataPath) 
                : appData;

            if (LogManager.Configuration is not null)
            {
                LogManager.Configuration.Variables["configDir"] = configDir;
            }           
        });
    }
}
