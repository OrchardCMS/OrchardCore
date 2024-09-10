using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.HomeRoute.Routing;
using OrchardCore.Modules;
using OrchardCore.Routing;

namespace OrchardCore.HomeRoute;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.HomeRoute;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<HomeRouteTransformer>();
        services.AddSingleton<IShellRouteValuesAddressScheme, HomeRouteValuesAddressScheme>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapDynamicControllerRoute<HomeRouteTransformer>("/");
    }
}
