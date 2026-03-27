using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Search;
using OrchardCore.AzureAI.Core;
using OrchardCore.AzureAI.Deployment;
using OrchardCore.AzureAI.Drivers;
using OrchardCore.AzureAI.Flows;
using OrchardCore.AzureAI.Handlers;
using OrchardCore.AzureAI.Recipes;
using OrchardCore.AzureAI.Services;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.AzureAI.Migrations;

namespace OrchardCore.AzureAI;

public sealed class Startup : StartupBase
{
    internal readonly IStringLocalizer S;

    public Startup(IStringLocalizer<Startup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIndexProfileHandler<AzureAISearchIndexProfileHandler>();
        services.AddIndexProfileHandler<AzureAISearchIndexHandler>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddAzureAISearchServices();
        services.AddSiteDisplayDriver<AzureAISearchDefaultSettingsDisplayDriver>();
        services.AddDataMigration<AzureAISearchIndexSettingsMigrations>();
    }
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSearchService<AzureAISearchService>(AzureAISearchConstants.ProviderName);
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
    internal readonly IStringLocalizer S;

    public ContentsStartup(IStringLocalizer<ContentsStartup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<IndexProfile, AzureAISearchIndexProfileDisplayDriver>();

        services
            .AddIndexProfileHandler<AzureAISearchContentIndexProfileHandler>()
            .AddAzureAISearchIndexingSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in Azure AI Search"];
                o.Description = S["Create an Azure AI Search index based on site contents."];
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

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AzureAISearchIndexDeploymentSource, AzureAISearchIndexDeploymentStep, AzureAISearchIndexDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexRebuildDeploymentSource, AzureAISearchIndexRebuildDeploymentStep, AzureAISearchIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexResetDeploymentSource, AzureAISearchIndexResetDeploymentStep, AzureAISearchIndexResetDeploymentStepDriver>();
    }
}

[RequireFeatures("OrchardCore.Flows")]
public sealed class FlowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAzureAISearchFieldIndexEvents, BagPartAzureAISearchFieldIndexEvents>();
    }
}

[Feature("OrchardCore.Search.AzureAI")]
public sealed class LegacyFeatureStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<LegacyFeatureMigrations>();
    }
}

internal sealed class LegacyFeatureMigrations : DataMigration
{
    public static int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            if (await featuresManager.IsFeatureEnabledAsync("OrchardCore.AzureAI"))
            {
                return;
            }

            await featuresManager.EnableFeaturesAsync("OrchardCore.AzureAI");
        });

        return 1;
    }
}
