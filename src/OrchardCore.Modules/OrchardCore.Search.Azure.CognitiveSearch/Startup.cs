using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Azure.CognitiveSearch.Controllers;
using OrchardCore.Search.Azure.CognitiveSearch.Drivers;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.Azure.CognitiveSearch;

public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly AdminOptions _adminOptions;

    public Startup(IShellConfiguration shellConfiguration, IOptions<AdminOptions> adminOptions)
    {
        _shellConfiguration = shellConfiguration;
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureCognitiveSearchServices(_shellConfiguration);
        services.AddScoped<IDisplayDriver<ISite>, AzureCognitiveSearchSettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var adminControllerName = typeof(AdminController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "AzureCognitiveSearch.Index",
            areaName: "OrchardCore.Search.Azure.CognitiveSearch",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-cognitive-search/Index",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "AzureCognitiveSearch.Delete",
            areaName: "OrchardCore.Search.Azure.CognitiveSearch",
            pattern: _adminOptions.AdminUrlPrefix + "/azure-cognitive-search/Delete/{id}",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
        );
    }
}

[RequireFeatures("OrchardCore.Search")]
public class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISearchService, AzureCognitiveSearchService>();
    }
}
