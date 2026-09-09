using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Features.Deployment;
using OrchardCore.Features.Recipes.Executors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Features.Drivers;
using OrchardCore.Features.Models;
using OrchardCore.Features.Services;
using OrchardCore.Modules;
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
        services.AddRecipeExecutionStep<FeatureStep>();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddNavigationProvider<AdminMenu>();

        // Builds the rows of the features admin list.
        services.AddDisplayDriver<FeatureEntry, FeatureEntryDisplayDriver>();

        services.AddDeployment<AllFeaturesDeploymentSource, AllFeaturesDeploymentStep, AllFeaturesDeploymentStepDriver>();
    }
}
