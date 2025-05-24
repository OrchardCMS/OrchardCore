using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Migrations;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI;

public sealed class Startup : StartupBase
{
    internal readonly IStringLocalizer S;

    public Startup(IStringLocalizer<Startup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, AzureAISearchIndexHandler>());
        services.AddNavigationProvider<AdminMenu>();
        services.AddAzureAISearchServices();
        services.AddSiteDisplayDriver<AzureAISearchDefaultSettingsDisplayDriver>();

        services.AddDisplayDriver<IndexEntity, AzureAISearchIndexEntityDisplayDriver>();

        services.AddDataMigration<AzureAISearchIndexSettingsMigrations>();

        services.Configure<IndexingOptions>(options =>
        {
            options.AddIndexingSource(AzureAISearchConstants.ProviderName, IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in Azure AI Search"];
                o.Description = S["Create an Azure AI Search index based on site contents."];
            });
        });
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddKeyedScoped<ISearchService, AzureAISearchService>(AzureAISearchConstants.ProviderName);
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

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsStartup : StartupBase
{
    private readonly IStringLocalizer S;

    public ContentsStartup(IStringLocalizer<ContentsStartup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, AzureAISearchContentIndexEntityHandler>());
        services.Configure<AzureAISearchOptions>(options =>
        {
            options.AddIndexSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Contents"];
                o.Description = S["Create an index based on content items."];
            });
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AzureAISearchSettingsDeploymentSource, AzureAISearchSettingsDeploymentStep, AzureAISearchSettingsDeploymentStepDriver>();
    }
}
