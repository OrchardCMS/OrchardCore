using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Deployments;
using OrchardCore.Indexing.Drivers;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.Recipes;
using OrchardCore.Indexing.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;

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
        services.AddDisplayDriver<IndexEntity, ContentIndexEntityDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<IndexEntityStep>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<IndexEntityDeploymentSource, IndexEntityDeploymentStep, IndexEntityDeploymentStepDisplayDriver>();
    }
}
