using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Handlers;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Recipes;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Drivers;
using OrchardCore.Search.Elasticsearch.Migrations;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ElasticsearchConnectionOptions>, ElasticsearchConnectionOptionsConfigurations>();
        services.AddTransient<IElasticsearchClientFactory, ElasticsearchClientFactory>();
        services.AddSingleton((sp) =>
        {
            var factory = sp.GetRequiredService<IElasticsearchClientFactory>();
            var options = sp.GetRequiredService<IOptions<ElasticsearchConnectionOptions>>().Value;

            return factory.Create(options);
        });

        services.Configure<ElasticsearchOptions>(options =>
        {
            var configuration = _shellConfiguration.GetSection(ElasticsearchConnectionOptionsConfigurations.ConfigSectionName);

            options.AddIndexPrefix(configuration);
            options.AddTokenFilters(configuration);
            options.AddAnalyzers(configuration);
        });

        services.AddElasticsearchServices();
        services.AddPermissionProvider<PermissionProvider>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddDisplayDriver<Query, ElasticsearchQueryDisplayDriver>();
        services.AddDataMigration<ElasticsearchQueryMigrations>();
        services.AddScoped<IQueryHandler, ElasticsearchQueryHandler>();

        services.AddDisplayDriver<IndexProfile, ElasticsearchIndexProfileDisplayDriver>();

        services.AddIndexProfileHandler<ElasticsearchIndexProfileHandler>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<ElasticsearchIndexStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<ElasticsearchIndexResetStep>();
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
            .AddIndexProfileHandler<ElasticsearchContentIndexProfileHandler>()
            .AddElasticsearchIndexingSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in Elasticsearch"];
                o.Description = S["Create an Elasticsearch index based on site contents."];
            });
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSearchService<ElasticsearchService>(ElasticsearchConstants.ProviderName);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<ElasticsearchIndexDeploymentSource, ElasticsearchIndexDeploymentStep, ElasticIndexDeploymentStepDriver>();
        services.AddDeployment<ElasticsearchIndexRebuildDeploymentSource, ElasticsearchIndexRebuildDeploymentStep, ElasticIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<ElasticsearchIndexResetDeploymentSource, ElasticsearchIndexResetDeploymentStep, ElasticIndexResetDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Search.Elasticsearch.ContentPicker")]
public sealed class ElasticContentPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentPickerResultProvider, ElasticsearchContentPickerResultProvider>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldElasticEditorSettingsDriver>();
        services.AddShapeAttributes<ElasticContentPickerShapeProvider>();
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
