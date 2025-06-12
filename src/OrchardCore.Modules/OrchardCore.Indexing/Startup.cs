using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Indexing.DataMigrations;
using OrchardCore.Indexing.Deployments;
using OrchardCore.Indexing.Drivers;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Search.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIndexingCore();
        services.AddDataMigration<Migrations>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddDisplayDriver<IndexProfile, IndexProfileDisplayDriver>();
        services.AddPermissionProvider<IndexingPermissionsProvider>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IContentHandler, IndexingContentHandler>());
        services.AddScoped<IContentHandler, CreateIndexingTaskContentHandler>();
        services.TryAddScoped<ContentIndexingService>();
        services.AddIndexProfileHandler<ContentIndexProfileHandler>();
        services.AddDisplayDriver<IndexProfile, ContentIndexProfileDisplayDriver>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IModularTenantEvents, ContentIndexInitializerService>());
        services.AddDataMigration<WorkerFeatureMigrations>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<CreateOrUpdateIndexProfileStep>();
        services.AddRecipeExecutionStep<ResetIndexStep>();
        services.AddRecipeExecutionStep<RebuildIndexStep>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<IndexProfileDeploymentSource, IndexProfileDeploymentStep, IndexProfileDeploymentStepDisplayDriver>();
        services.AddDeployment<RebuildIndexProfileDeploymentSource, RebuildIndexProfileDeploymentStep, RebuildIndexProfileDeploymentStepDriver>();
        services.AddDeployment<ResetIndexProfileDeploymentSource, ResetIndexProfileDeploymentStep, ResetIndexProfileDeploymentStepDriver>();
    }
}

[Feature(IndexingConstants.Feature.Worker)]
public sealed class WorkerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTask, ContentIndexingBackgroundTask>();
    }
}
