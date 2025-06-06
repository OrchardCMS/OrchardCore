using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Core;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Recipes;
using OrchardCore.Search.Lucene.Core.Handlers;
using OrchardCore.Search.Lucene.DataMigrations;
using OrchardCore.Search.Lucene.Deployment;
using OrchardCore.Search.Lucene.Drivers;
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
        services.TryAddSingleton<ILuceneIndexStore, LuceneIndexStore>();
        services.TryAddSingleton<ILuceneIndexingState, LuceneIndexingState>();
        services.TryAddSingleton<LuceneAnalyzerManager>();
        services.TryAddScoped<ILuceneSearchQueryService, LuceneSearchQueryService>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();

        services.Configure<LuceneOptions>(o =>
            o.Analyzers.Add(new LuceneAnalyzer(LuceneConstants.DefaultAnalyzer,
                new StandardAnalyzer(LuceneConstants.DefaultVersion))));

        services.AddDisplayDriver<Query, LuceneQueryDisplayDriver>();

        services
            .AddLuceneQueries()
            .AddQuerySource<LuceneQuerySource>(LuceneQuerySource.SourceName);

        services.AddDataMigration<LuceneQueryMigrations>();
        services.AddScoped<IQueryHandler, LuceneQueryHandler>();

        services.AddDisplayDriver<IndexProfile, LuceneIndexProfileDisplayDriver>();

        services.AddIndexProfileHandler<LuceneIndexProfileHandler>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<LuceneIndexStep>();
        services.AddRecipeExecutionStep<LuceneIndexRebuildStep>();
        services.AddRecipeExecutionStep<LuceneIndexResetStep>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsStartup : StartupBase
{
    internal readonly IStringLocalizer S;

    public ContentsStartup(IStringLocalizer<ContentsStartup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<IndexingMigrations>();

        services
            .AddIndexProfileHandler<LuceneContentIndexProfileHandler>()
            .AddLuceneIndexingSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in Lucene"];
                o.Description = S["Create an Lucene index based on site contents."];
            });
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSearchService<LuceneSearchService>(LuceneConstants.ProviderName);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<LuceneIndexDeploymentSource, LuceneIndexDeploymentStep, LuceneIndexDeploymentStepDriver>();
        services.AddDeployment<LuceneIndexRebuildDeploymentSource, LuceneIndexRebuildDeploymentStep, LuceneIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<LuceneIndexResetDeploymentSource, LuceneIndexResetDeploymentStep, LuceneIndexResetDeploymentStepDriver>();
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
