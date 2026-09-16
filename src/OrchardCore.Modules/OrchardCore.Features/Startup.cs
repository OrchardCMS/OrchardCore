using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Features.Deployment;
using OrchardCore.Features.Recipes.Executors;
using OrchardCore.Features.Services;
using OrchardCore.Features.Endpoints.Management;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteManagementCapabilityProvider, FeatureRemoteManagementCapabilityProvider>();
        services.AddRecipeExecutionStep<FeatureStep>();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<FeatureService>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddDeployment<AllFeaturesDeploymentSource, AllFeaturesDeploymentStep, AllFeaturesDeploymentStepDriver>();
        services.AddSingleton<IDeploymentStepDefinition>(new BooleanDeploymentStepDefinition<AllFeaturesDeploymentStep>(
            nameof(AllFeaturesDeploymentStep), "ignoreDisabledFeatures", step => step.IgnoreDisabledFeatures, (step, value) => step.IgnoreDisabledFeatures = value));
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddFeatureManagementEndpoints();
    }
}
