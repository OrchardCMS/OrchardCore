using Microsoft.Extensions.Hosting;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web;

namespace OrchardCore.Logging
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseNLogWeb(this IHostBuilder builder)
        {
            LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
            builder.UseNLog();
            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                var environment = context.HostingEnvironment;
                NLog.LogManager.Setup()
                    .LoadConfigurationFromFile(System.IO.Path.Combine(environment.ContentRootPath, "NLog.config"))
                    .LoadConfiguration(configBuilder => configBuilder.Configuration.Variables["configDir"] = environment.ContentRootPath);
            });

            return builder;
        }
    }
}
