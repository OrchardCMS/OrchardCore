using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using OrchardVNext.Environment;
using System;

namespace OrchardVNext.Web {
    public class Startup {
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            return OrchardStarter.ConfigureHost(services);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) {
            var host = OrchardStarter.CreateHost(app, loggerFactory);
            host.Initialize();
        }
    }
}