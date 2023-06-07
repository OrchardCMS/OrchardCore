using System.IO;
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
                LogManager.Configuration.Variables["configDir"] = environment.ContentRootPath;
            });
    }
}

internal static class AspNetExtensions
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
