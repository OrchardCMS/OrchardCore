using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Recipes;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Deployment;
using OrchardCore.Search.Lucene.Drivers;
using OrchardCore.Search.Lucene.Handler;
using OrchardCore.Search.Lucene.Handlers;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Recipes;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.Settings;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Lucene;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddSingleton<LuceneIndexingState>();
        services.AddSingleton<LuceneIndexSettingsService>();
        services.AddSingleton<LuceneIndexManager>();
        services.AddSingleton<LuceneAnalyzerManager>();
        services.AddScoped<LuceneIndexingService>();
        services.AddScoped<IModularTenantEvents, LuceneIndexInitializerService>();
        services.AddScoped<ILuceneSearchQueryService, LuceneSearchQueryService>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();

        services.Configure<LuceneOptions>(o =>
            o.Analyzers.Add(new LuceneAnalyzer(LuceneSettings.StandardAnalyzer,
                new StandardAnalyzer(LuceneSettings.DefaultVersion))));

        services.AddDisplayDriver<Query, LuceneQueryDisplayDriver>();
        services.AddScoped<IContentHandler, LuceneIndexingContentHandler>();

        services.AddLuceneQueries()
            .AddQuerySource<LuceneQuerySource>(LuceneQuerySource.SourceName);

        services.AddRecipeExecutionStep<LuceneIndexStep>();
        services.AddRecipeExecutionStep<LuceneIndexRebuildStep>();
        services.AddRecipeExecutionStep<LuceneIndexResetStep>();
        services.AddScoped<IAuthorizationHandler, LuceneAuthorizationHandler>();
        services.AddDataMigration<LuceneQueryMigrations>();
        services.AddScoped<IQueryHandler, LuceneQueryHandler>();
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISearchService, LuceneSearchService>();
        services.AddSiteDisplayDriver<LuceneSettingsDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, LuceneAuthorizationHandler>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<LuceneIndexDeploymentSource, LuceneIndexDeploymentStep, LuceneIndexDeploymentStepDriver>();
        services.AddDeployment<LuceneSettingsDeploymentSource, LuceneSettingsDeploymentStep, LuceneSettingsDeploymentStepDriver>();
        services.AddDeployment<LuceneIndexRebuildDeploymentSource, LuceneIndexRebuildDeploymentStep, LuceneIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<LuceneIndexResetDeploymentSource, LuceneIndexResetDeploymentStep, LuceneIndexResetDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Search.Lucene.Worker")]
public sealed class LuceneWorkerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
    }
}

[Feature("OrchardCore.Search.Lucene.ContentPicker")]
public sealed class LuceneContentPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentPickerResultProvider, LuceneContentPickerResultProvider>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldLuceneEditorSettingsDriver>();
        services.AddShapeAttributes<LuceneContentPickerShapeProvider>();
    }
}

[RequireFeatures("OrchardCore.ContentTypes")]
public sealed class ContentTypesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
    }
}
