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
using OrchardCore.Indexing.Deployments;
using OrchardCore.Indexing.Drivers;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Search.Indexing.Core;

namespace OrchardCore.Indexing;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIndexingCore();
        services.AddScoped<IIndexingTaskManager, IndexingTaskManager>();
        services.AddScoped<IContentHandler, CreateIndexingTaskContentHandler>();
        services.AddDataMigration<Migrations>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddDisplayDriver<IndexEntity, IndexEntityDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddScoped<ContentIndexingService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, ContentIndexEntryHandler>());
        services.AddDisplayDriver<IndexEntity, ContentIndexEntityDisplayDriver>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IModularTenantEvents, ContentIndexInitializerService>());
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<IndexEntityStep>();
        services.AddRecipeExecutionStep<ResetIndexEntityStep>();
        services.AddRecipeExecutionStep<RebuildIndexEntityStep>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<IndexEntityDeploymentSource, IndexEntityDeploymentStep, IndexEntityDeploymentStepDisplayDriver>();
        services.AddDeployment<RebuildIndexEntityDeploymentSource, RebuildIndexEntityDeploymentStep, RebuildIndexEntityDeploymentStepDriver>();
        services.AddDeployment<ResetIndexEntityDeploymentSource, ResetIndexEntityDeploymentStep, ResetIndexEntityDeploymentStepDriver>();
    }
}

[Feature(IndexingConstants.Feature.Worker)]
public sealed class WorkerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
    }
}
