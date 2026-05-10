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
using OrchardCore.OpenSearch.Core.Deployment;
using OrchardCore.OpenSearch.Core.Handlers;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Core.Providers;
using OrchardCore.OpenSearch.Core.Recipes;
using OrchardCore.OpenSearch.Core.Services;
using OrchardCore.OpenSearch.Drivers;
using OrchardCore.OpenSearch.Migrations;
using OrchardCore.OpenSearch.Services;
using OrchardCore.Search;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenSearch;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<OpenSearchConnectionOptions>, OpenSearchConnectionOptionsConfigurations>();
        services.AddTransient<IOpenSearchClientFactory, OpenSearchClientFactory>();
        services.AddSingleton((sp) =>
        {
            var factory = sp.GetRequiredService<IOpenSearchClientFactory>();
            var options = sp.GetRequiredService<IOptions<OpenSearchConnectionOptions>>().Value;

            return factory.Create(options);
        });

        services.Configure<OpenSearchOptions>(options =>
        {
            var configuration = _shellConfiguration.GetSection(OpenSearchConnectionOptionsConfigurations.ConfigSectionName);

            options.AddIndexPrefix(configuration);
            options.AddTokenFilters(configuration);
            options.AddAnalyzers(configuration);
        });

        services.AddOpenSearchServices();
        services.AddPermissionProvider<PermissionProvider>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddDisplayDriver<Query, OpenSearchQueryDisplayDriver>();
        services.AddDataMigration<OpenSearchQueryMigrations>();
        services.AddScoped<IQueryHandler, OpenSearchQueryHandler>();

        services.AddDisplayDriver<IndexProfile, OpenSearchIndexProfileDisplayDriver>();

        services.AddIndexProfileHandler<OpenSearchIndexProfileHandler>();    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<OpenSearchIndexStep>();
        services.AddRecipeExecutionStep<OpenSearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<OpenSearchIndexResetStep>();
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
            .AddIndexProfileHandler<OpenSearchContentIndexProfileHandler>()
            .AddOpenSearchIndexingSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in OpenSearch"];
                o.Description = S["Create an OpenSearch index based on site contents."];
            });
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSearchService<OpenSearchService>(OpenSearchConstants.ProviderName);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<OpenSearchIndexDeploymentSource, OpenSearchIndexDeploymentStep, OpenSearchIndexDeploymentStepDriver>();
        services.AddDeployment<OpenSearchIndexRebuildDeploymentSource, OpenSearchIndexRebuildDeploymentStep, OpenSearchIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<OpenSearchIndexResetDeploymentSource, OpenSearchIndexResetDeploymentStep, OpenSearchIndexResetDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.OpenSearch.ContentPicker")]
public sealed class OpenSearchContentPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentPickerResultProvider, OpenSearchContentPickerResultProvider>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldOpenSearchEditorSettingsDriver>();
        services.AddShapeAttributes<OpenSearchContentPickerShapeProvider>();
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
