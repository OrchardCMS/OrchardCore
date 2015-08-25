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

            services.Configure<ExtensionHarvestingOptions>(options => {
                var expander = new ModuleLocationExpander(
                    DefaultExtensionTypes.Module,
                    new[] { "~/Core/OrchardVNext.Core", "~/Modules" },
                    "Module.txt"
                    );

                options.ModuleLocationExpanders.Add(expander);
            });

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory) {
            builder.ConfigureWebHost(loggerFactory);
            
            builder.InitializeHost();
        }
    }
}