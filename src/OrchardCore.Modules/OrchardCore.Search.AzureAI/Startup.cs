using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing.Core;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Migrations;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Recipes;
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
        services.AddAzureAISearchServices();
        services.AddSiteDisplayDriver<AzureAISearchDefaultSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddDisplayDriver<AzureAISearchIndexSettings, AzureAISearchIndexSettingsDisplayDriver>();
        services.AddScoped<IAzureAISearchIndexSettingsHandler, AzureAISearchIndexHandler>();

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

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<AzureAISearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<AzureAISearchIndexResetStep>();
        services.AddRecipeExecutionStep<AzureAISearchIndexSettingsStep>();
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<AzureAISearchSettingsDisplayDriver>();
        services.AddScoped<ISearchService, AzureAISearchService>();
        services.AddScoped<IAuthorizationHandler, AzureAISearchAuthorizationHandler>();
    }
}

[RequireFeatures("OrchardCore.ContentTypes")]
public sealed class ContentTypesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, AzureAISearchAuthorizationHandler>();
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
        services.AddDisplayDriver<AzureAISearchIndexSettings, ContentAzureAISearchIndexSettingsDisplayDriver>();
        services.AddScoped<IAzureAISearchIndexSettingsHandler, ContentAzureAISearchIndexHandler>();
        services.Configure<AzureAISearchOptions>(options =>
        {
            options.AddIndexSource(AzureAISearchConstants.ContentsIndexSource, o =>
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
        services.AddDeployment<AzureAISearchIndexDeploymentSource, AzureAISearchIndexDeploymentStep, AzureAISearchIndexDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchSettingsDeploymentSource, AzureAISearchSettingsDeploymentStep, AzureAISearchSettingsDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexRebuildDeploymentSource, AzureAISearchIndexRebuildDeploymentStep, AzureAISearchIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexResetDeploymentSource, AzureAISearchIndexResetDeploymentStep, AzureAISearchIndexResetDeploymentStepDriver>();
    }
}
