using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Shell;
using Orchard.Hosting;
using Orchard.Hosting.Mvc;

namespace Orchard.Cms
{
    public static class OrchardExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            services.AddWebHost();

            services.AddTheming();
            services.AddThemeFolder("Themes");
            services.ConfigureShell("Sites");

            services.AddOrchardMvc();
            services.AddModuleFolder("Modules");

            // Save the list of service definitions
            services.AddSingleton(_ => services);

            return services;
        }

        public static IApplicationBuilder UseOrchardCms(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ConfigureWebHost(env, loggerFactory);

            return app;
        }
    }
}