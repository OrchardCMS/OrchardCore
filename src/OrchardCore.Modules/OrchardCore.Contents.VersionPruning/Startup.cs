using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Contents.VersionPruning.Controllers;
using OrchardCore.Contents.VersionPruning.Drivers;
using OrchardCore.Contents.VersionPruning.Services;
using OrchardCore.Contents.VersionPruning.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.VersionPruning;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentVersionPruningService, ContentVersionPruningService>();
        services.AddSingleton<IBackgroundTask, ContentVersionPruningBackgroundTask>();
        services.AddSiteDisplayDriver<ContentVersionPruningSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var controllerName = typeof(AdminController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "ContentsVersionPruningPrune",
            areaName: "OrchardCore.Contents.VersionPruning",
            pattern: "Contents/VersionPruning/Prune",
            defaults: new { controller = controllerName, action = nameof(AdminController.Prune) }
        );
    }
}
