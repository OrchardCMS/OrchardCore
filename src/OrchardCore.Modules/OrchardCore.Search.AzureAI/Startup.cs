using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Controllers;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI;

public class Startup(ILogger<Startup> logger, IShellConfiguration shellConfiguration, IOptions<AdminOptions> adminOptions)
    : StartupBase
{
    private readonly ILogger _logger = logger;
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;
    private readonly AdminOptions _adminOptions = adminOptions.Value;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddAzureAISearchServices(_shellConfiguration, _logger);
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IDisplayDriver<ISite>, AzureAISearchDefaultSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var adminControllerName = typeof(AdminController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Index",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Index",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Create",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Create",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Edit",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Edit/{indexName}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Delete",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Delete/{indexName}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Reset",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Reset/{indexName}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Reset) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Rebuild",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Rebuild/{indexName}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Rebuild) }
        );
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
        services.AddTransient<IDeploymentSource, AzureAISearchIndexDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AzureAISearchIndexDeploymentStep>());
        services.AddScoped<IDisplayDriver<DeploymentStep>, AzureAISearchIndexDeploymentStepDriver>();

        services.AddTransient<IDeploymentSource, AzureAISearchSettingsDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AzureAISearchSettingsDeploymentStep>());
        services.AddScoped<IDisplayDriver<DeploymentStep>, AzureAISearchSettingsDeploymentStepDriver>();

        services.AddTransient<IDeploymentSource, AzureAISearchIndexRebuildDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AzureAISearchIndexRebuildDeploymentStep>());
        services.AddScoped<IDisplayDriver<DeploymentStep>, AzureAISearchIndexRebuildDeploymentStepDriver>();

        services.AddTransient<IDeploymentSource, AzureAISearchIndexResetDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AzureAISearchIndexResetDeploymentStep>());
        services.AddScoped<IDisplayDriver<DeploymentStep>, AzureAISearchIndexResetDeploymentStepDriver>();
    }
}
