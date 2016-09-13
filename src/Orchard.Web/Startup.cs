using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Shell;
using Orchard.Hosting;
using Orchard.Hosting.Mvc;
using System;
using Microsoft.AspNetCore.Hosting;

namespace Orchard.Web
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddWebHost();

            services.ConfigureShell("Sites");

            services.AddModuleFolder("Modules");
            services.AddThemeFolder("Themes");

            services.AddOrchardMvc();

            // Save the list of service definitions
            services.AddSingleton(_ => services);

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder builder, IHostingEnvironment hostingEnvrionmnent, ILoggerFactory loggerFactory)
        {
            builder.ConfigureWebHost(hostingEnvrionmnent, loggerFactory);
        }
    }
}