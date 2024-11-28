using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Drivers;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Search.Lucene.Handler;
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
        services.AddSingleton((sp) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticsearchConnectionOptions>>().Value;

            return ElasticsearchClientFactory.Create(options);
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
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISearchService, ElasticsearchService>();
        services.AddSiteDisplayDriver<ElasticSettingsDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, ElasticsearchAuthorizationHandler>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<ElasticsearchIndexDeploymentSource, ElasticsearchIndexDeploymentStep, ElasticIndexDeploymentStepDriver>();
        services.AddDeployment<ElasticSettingsDeploymentSource, ElasticSettingsDeploymentStep, ElasticSettingsDeploymentStepDriver>();
        services.AddDeployment<ElasticsearchIndexRebuildDeploymentSource, ElasticsearchIndexRebuildDeploymentStep, ElasticIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<ElasticsearchIndexResetDeploymentSource, ElasticsearchIndexResetDeploymentStep, ElasticIndexResetDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Search.Elasticsearch.Worker")]
public sealed class ElasticWorkerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
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
