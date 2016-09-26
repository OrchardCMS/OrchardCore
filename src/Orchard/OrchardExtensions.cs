using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Shell;
using Orchard.Hosting;
using Orchard.Hosting.Mvc;

namespace Orchard
{
    public static class OrchardExtensions
    {
        public static IServiceCollection AddOrchard(this IServiceCollection services)
        {
            services.AddWebHost();

            var hostingEnvironment = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            services.ConfigureShell(hostingEnvironment.ContentRootFileProvider, "Sites");

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