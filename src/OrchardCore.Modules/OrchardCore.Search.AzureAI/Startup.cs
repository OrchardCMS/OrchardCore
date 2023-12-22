using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Controllers;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Models;
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
        if (!services.TryAddAzureAISearchServices(_shellConfiguration, _logger))
        {
            return;
        }

        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<AzureAISearchDefaultOptions>>().Value;

        if (!options.IsConfigurationExists())
        {
            return;
        }

        var adminControllerName = typeof(AdminController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Index",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Index",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureAISearch.Delete",
            areaName: "OrchardCore.Search.AzureAI",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-search/Delete/{id}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
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
    }
}

[RequireFeatures("OrchardCore.ContentTypes")]
public class ContentTypesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
    }
}

