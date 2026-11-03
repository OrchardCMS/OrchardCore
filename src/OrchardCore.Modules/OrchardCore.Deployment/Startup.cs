using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Endpoints.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.BackgroundTasks;
using OrchardCore.Admin;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Core;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Deployment.Operations;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Drivers;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.Recipes;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.Steps;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.FileStorage;
using OrchardCore.Modules;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.RemoteManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddDeploymentPlanEndpoints();
        routes.AddDeploymentArtifactEndpoints();
        routes.AddDeploymentOperationEndpoints();
        routes.AddDeploymentStepTypeEndpoints();
        routes.AddDeploymentStepEndpoints();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient<FileCreationService>();
        services.AddDeploymentServices();
        services.Configure<DeploymentPackageOptions>(_shellConfiguration.GetSection("Deployment:Package"));
        services.Configure<DeploymentOperationOptions>(_shellConfiguration.GetSection("Deployment:Operations"));
        services.Configure<DeploymentArtifactOptions>(_shellConfiguration.GetSection("Deployment:Artifacts"));
        services.AddScoped<DeploymentArtifactStore>();
        services.AddScoped<DeploymentOperationStore>();
        services.AddScoped<IDeploymentOperationExecutor, DeploymentOperationExecutor>();
        services.AddScoped<DeploymentOperationRunner>();
        services.AddSingleton<IBackgroundTask, DeploymentOperationTask>();
        services.AddSingleton<IBackgroundTask, DeploymentArtifactCleanupTask>();

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
        services.AddScoped<DeploymentStepRegistry>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, DeploymentManagementCapabilityProvider>();
        foreach (var type in new[] { nameof(RecipeFileDeploymentStep), nameof(CustomFileDeploymentStep), nameof(JsonRecipeDeploymentStep), nameof(DeploymentPlanDeploymentStep) })
        {
            services.AddSingleton<IDeploymentStepDefinition>(new BuiltInDeploymentStepDefinition(type));
        }

        // Builds the rows of the deployment plans admin list.
        services.AddDisplayDriver<DeploymentPlanEntry, DeploymentPlanEntryDisplayDriver>();

        services.AddRecipeExecutionStep<DeploymentPlansRecipeStep>();

        services.AddDeployment<DeploymentPlanDeploymentSource, DeploymentPlanDeploymentStep, DeploymentPlanDeploymentStepDriver>();

        services.AddDeployment<JsonRecipeDeploymentSource, JsonRecipeDeploymentStep, JsonRecipeDeploymentStepDriver>();

    }
}
