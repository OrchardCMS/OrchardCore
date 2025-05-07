using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Web;

namespace OrchardCore.Logging;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseNLogWeb(this IWebHostBuilder builder)
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
                    LogManager.Configuration = new LoggingConfiguration();
                }               

                LogManager.Configuration.Variables["configDir"] = configDir;
            });
    }
}

public static class AspNetExtensions
{
    public static LoggingConfiguration ConfigureNLog(this IHostEnvironment env, string configFileRelativePath)
    {
        var fileName = Path.Combine(env.ContentRootPath, configFileRelativePath);

        LogManager.Setup()
            .SetupLogFactory(factory => factory.AddCallSiteHiddenAssembly(typeof(AspNetExtensions).GetType().Assembly))
            .SetupExtensions(ext =>
            {
                ext.RegisterLayoutRenderer<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
                ext.RegisterNLogWeb();
            })
            .LoadConfigurationFromFile(fileName);

        return LogManager.Configuration;
    }
}
