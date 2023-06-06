using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Web;

namespace OrchardCore.Logging
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseNLogWeb(this IWebHostBuilder builder)
        {
            LogManager.Setup().SetupExtensions(builder =>
                builder.RegisterLayoutRenderer<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName));

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

    internal static class AspNetExtensions
    {
        public static LoggingConfiguration ConfigureNLog(this IHostEnvironment env, string configFileRelativePath)
        {
            LogManager.Setup().SetupExtensions(builder =>
                builder.RegisterAssembly(typeof(AspNetExtensions).GetType().Assembly));

            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);
            var fileName = Path.Combine(env.ContentRootPath, configFileRelativePath);
            LogManager.Setup().LoadConfigurationFromFile(fileName);
            return LogManager.Configuration;
        }
    }
}
