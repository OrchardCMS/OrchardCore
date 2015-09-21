using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Orchard.Environment.Extensions.Folders;
using Orchard.Hosting;
using System;

namespace Orchard.Web {
    public class Startup {
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            services
                .AddWebHost();

            services.AddModuleFolder("~/Core/Orchard.Core");
            services.AddModuleFolder("~/Modules");

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory, IOrchardHost orchardHost) {
            builder.ConfigureWebHost(loggerFactory);

            orchardHost.Initialize();
        }
    }
}