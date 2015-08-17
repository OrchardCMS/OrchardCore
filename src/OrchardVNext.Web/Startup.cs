using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using OrchardVNext.Hosting;
using System;

namespace OrchardVNext.Web {
    public class Startup {
        public IServiceProvider ConfigureServices(IServiceCollection services) {
            return services
                .AddWeb()
                .BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory) {
            builder.ConfigureWeb(loggerFactory);
            
            builder.InitializeHost();
        }
    }
}