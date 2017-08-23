using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.LayoutRenderers;
using NLog.Web;

namespace Orchard.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddNLogWeb(this ILoggingBuilder logging, IHostingEnvironment env)
        {
            env.ConfigureNLog($"{env.ContentRootPath}{Path.DirectorySeparatorChar}NLog.config");
            LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
            logging.AddNLog();
            LogManager.Configuration.Variables["configDir"] = env.ContentRootPath;

            return logging;
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder logging)
        {
            return AddNLog(logging, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder logging, NLogProviderOptions options)
        {
            //ignore this
            LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging")));
            LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Abstractions")));

            try
            {
                //try the Filter ext
                var filterAssembly = Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Filter"));
                LogManager.AddHiddenAssembly(filterAssembly);
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                //ignore
            }

            LogManager.AddHiddenAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);

            using (var provider = new NLogLoggerProvider(options))
            {
                logging.AddProvider(provider);
            }
            return logging;
        }
    }
}
