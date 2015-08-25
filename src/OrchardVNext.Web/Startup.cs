using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using OrchardVNext.Hosting;
using OrchardVNext.Hosting.Extensions.Folders;
using OrchardVNext.Hosting.Extensions.Models;
using System;

namespace OrchardVNext.Web {
    public class Startup {
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            services
                .AddWebHost();

            services.AddModuleFolder("~/Core/OrchardVNext.Core");
            services.AddModuleFolder("~/Modules");

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory) {
            builder.ConfigureWebHost(loggerFactory);
            
            builder.InitializeHost();
        }
    }
}