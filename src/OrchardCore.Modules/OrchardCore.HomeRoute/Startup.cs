using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.HomeRoute.Routing;
using OrchardCore.Modules;
using OrchardCore.Routing;

namespace OrchardCore.HomeRoute
{
    public class Startup : StartupBase
    {
        public override int Order => -150;

        public override void ConfigureServices(IServiceCollection services)
        {
            // c.f. https://github.com/aspnet/AspNetCore/issues/12915
            //services.AddSingleton<HomeRouteTransformer>();

            services.AddSingleton<IShellRouteValuesAddressScheme, HomeRouteValuesAddressScheme>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // c.f. https://github.com/aspnet/AspNetCore/issues/12915
            //routes.MapDynamicControllerRoute<HomeRouteTransformer>("");

            app.UseMiddleware<HomeRouteMiddleware>();
        }
    }
}
