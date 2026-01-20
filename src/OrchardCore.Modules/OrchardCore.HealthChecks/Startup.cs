using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.HealthChecks
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var healthCheckOptions = serviceProvider.GetService<IOptions<HealthChecksOptions>>().Value;

            routes.MapHealthChecks(healthCheckOptions.Url);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.Configure<HealthChecksOptions>(_shellConfiguration.GetSection("OrchardCore_HealthChecks"));
        }
    }
}
