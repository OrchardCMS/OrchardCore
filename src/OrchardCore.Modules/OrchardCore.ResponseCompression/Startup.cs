using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.ResponseCompression
{
    public class Startup : StartupBase
    {
        public override int Order => -5;

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseResponseCompression();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options => options.EnableForHttps = true);
        }
    }
}
