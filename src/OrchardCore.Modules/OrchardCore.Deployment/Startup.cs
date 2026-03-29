using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Core;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Drivers;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Recipes;
using OrchardCore.Deployment.Steps;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeploymentServices();

        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();

        services.AddSingleton<IDeploymentTargetProvider, FileDownloadDeploymentTargetProvider>();

        // Register the fallback type for unknown deployment steps (e.g., when a feature is disabled).
        services.AddJsonDerivedTypeFallback<DeploymentStep, UnknownDeploymentStep>();
        services.AddDisplayDriver<DeploymentStep, UnknownDeploymentStepDriver>();

        // Custom File deployment step
        services.AddDeployment<CustomFileDeploymentSource, CustomFileDeploymentStep, CustomFileDeploymentStepDriver>();

        // Recipe File deployment step
        services.AddDeploymentWithoutSource<RecipeFileDeploymentStep, RecipeFileDeploymentStepDriver>();

        services.AddIndexProvider<DeploymentPlanIndexProvider>();
        services.AddDataMigration<Migrations>();

        services.AddScoped<IDeploymentPlanService, DeploymentPlanService>();

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddRecipeExecutionStep<DeploymentPlansRecipeStep>();
#pragma warning restore CS0618 // Type or member is obsolete
        services.AddRecipeDeploymentStep<DeploymentPlansRecipeDeploymentStep>();

        services.AddDeployment<DeploymentPlanDeploymentSource, DeploymentPlanDeploymentStep, DeploymentPlanDeploymentStepDriver>();

        services.AddDeployment<JsonRecipeDeploymentSource, JsonRecipeDeploymentStep, JsonRecipeDeploymentStepDriver>();

        // Register the factory resolver that auto-discovers exportable recipe steps.
        services.AddSingleton<IDeploymentStepFactoryResolver, DeploymentStepFactoryResolver>();

        // Register JSON type info for the generic RecipeStepDeploymentStep type.
        services.AddJsonDerivedTypeInfo<RecipeStepDeploymentStep, DeploymentStep>();

        // Register the generic recipe-step-based deployment source and driver.
        services.TryAddEnumerable(ServiceDescriptor.Transient<IDeploymentSource, RecipeStepDeploymentSource>());
        services.AddDisplayDriver<DeploymentStep, RecipeStepDeploymentStepDriver>();
    }
}
