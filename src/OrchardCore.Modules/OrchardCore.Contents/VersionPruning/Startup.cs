using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Contents.VersionPruning.Drivers;
using OrchardCore.Contents.VersionPruning.Services;
using OrchardCore.Contents.VersionPruning.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.VersionPruning;

[Feature("OrchardCore.Contents.VersionPruning")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentVersionPruningService, ContentVersionPruningService>();
        services.AddSingleton<IBackgroundTask, ContentVersionPruningBackgroundTask>();
        services.AddSiteDisplayDriver<ContentVersionPruningSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<ContentVersionPruningPermissionProvider>();
    }
}
