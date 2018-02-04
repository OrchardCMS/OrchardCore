using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Apis.OpenApi
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<OpenApiSettings>();

            services.Configure<MvcOptions>(options =>
            {
                options.OutputFormatters.Insert(0, new OpenApiJsonOutputFormatter());
                options.OutputFormatters.Insert(1, new OpenApiYamlOutputFormatter());
            });
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<OpenApiMiddleware>();
        }
    }
}
