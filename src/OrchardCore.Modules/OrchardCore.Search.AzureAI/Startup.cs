using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI;

public class Startup(ILogger<Startup> logger, IShellConfiguration shellConfiguration)
    : StartupBase
{
    private readonly ILogger _logger = logger;
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddAzureAISearchServices(_shellConfiguration, _logger);
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IDisplayDriver<ISite>, AzureAISearchDefaultSettingsDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Search")]
public class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<ISite>, AzureAISearchSettingsDisplayDriver>();
        services.AddScoped<ISearchService, AzureAISearchService>();
        services.AddScoped<IAuthorizationHandler, AzureAISearchAuthorizationHandler>();
    }
}

[RequireFeatures("OrchardCore.ContentTypes")]
public class ContentTypesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, AzureAISearchAuthorizationHandler>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AzureAISearchIndexDeploymentSource, AzureAISearchIndexDeploymentStep, AzureAISearchIndexDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchSettingsDeploymentSource, AzureAISearchSettingsDeploymentStep, AzureAISearchSettingsDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexRebuildDeploymentSource, AzureAISearchIndexRebuildDeploymentStep, AzureAISearchIndexRebuildDeploymentStepDriver>();
        services.AddDeployment<AzureAISearchIndexResetDeploymentSource, AzureAISearchIndexResetDeploymentStep, AzureAISearchIndexResetDeploymentStepDriver>();
    }
}
