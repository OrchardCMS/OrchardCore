using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Web;

namespace OrchardCore.Logging
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseNLogWeb(this IWebHostBuilder builder)
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

    internal static class AspNetExtensions
    {
        public static LoggingConfiguration ConfigureNLog(this IHostEnvironment env, string configFileRelativePath)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);
            var fileName = Path.Combine(env.ContentRootPath, configFileRelativePath);
            LogManager.LoadConfiguration(fileName);
            return LogManager.Configuration;
        }
    }
}
