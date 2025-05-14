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
            var configDir = string.IsNullOrWhiteSpace(appData) ? Path.Combine(environment.ContentRootPath, ShellOptionConstants.DefaultAppDataPath) : appData;

            if (LogManager.Configuration == null)
            {
                // Use ConfigureNLog to create a new LoggingConfiguration if none exists.
                // Use a default config file name, e.g., "NLog.config"
                LogManager.Configuration = environment.ConfigureNLog("NLog.config");
            }

            LogManager.Configuration.Variables["configDir"] = configDir;
        });
    }
}
