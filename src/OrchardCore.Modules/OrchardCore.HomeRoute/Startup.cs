using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.HomeRoute.Endpoints;
using OrchardCore.HomeRoute.Routing;
using OrchardCore.HomeRoute.Services;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.HomeRoute;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IHomeRouteService, HomeRouteService>();
        services.AddSingleton<HomeRouteTransformer>();
        services.AddSingleton<IShellRouteValuesAddressScheme, HomeRouteValuesAddressScheme>();
        services.AddPermissionProvider<Permissions>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, HomeRouteRemoteManagementCapabilityProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapDynamicControllerRoute<HomeRouteTransformer>("/");
        routes.AddHomeRouteManagementEndpoints();
    }
}
