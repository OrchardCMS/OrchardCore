using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.Apis.OpenApi
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());

            services.Configure<OpenApiOptions>(options => {
                options.Path = "/api.json";
            });

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
