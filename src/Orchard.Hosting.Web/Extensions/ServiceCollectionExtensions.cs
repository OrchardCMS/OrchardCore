using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Shell;
using Orchard.Hosting.Mvc;

namespace Orchard.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebHost(this IServiceCollection services)
        {
            return services.AddHost(internalServices =>
            {
                internalServices.AddLogging();
                internalServices.AddOptions();
                internalServices.AddLocalization();
                internalServices.AddHostCore();
                internalServices.AddExtensionManager("app_data", "dependencies");
                internalServices.AddCommands();

                internalServices.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });
        }


        public static IServiceCollection AddOrchard(this IServiceCollection services)
        {
            services.AddWebHost();

            var hostingEnvironment = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            services.ConfigureShell(
                "app_data",
                "sites");

            services.AddOrchardMvc();
            services.AddModuleFolder("Modules");

            // Save the list of service definitions
            services.AddSingleton(_ => services);

            return services;
        }

        public static IApplicationBuilder UserOrchard(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ConfigureWebHost(env, loggerFactory);

            return app;
        }
    }
}