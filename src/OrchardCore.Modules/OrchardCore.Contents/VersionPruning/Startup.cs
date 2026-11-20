using Microsoft.Extensions.DependencyInjection;
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
        services.AddJob<ContentVersionPruningJob>(ContentVersionPruningJob.Name, job =>
        {
            job.Title = ContentsLocalizationSources.ContentVersionPruningJobTitle;
            job.Description = ContentsLocalizationSources.ContentVersionPruningJobDescription;
            job.Schedule = "0 0 * * *";

            // Pruning deletes data, so it only runs once its schedule is enabled.
            job.ScheduleEnabled = false;
        });
        services.AddSiteDisplayDriver<ContentVersionPruningSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<ContentVersionPruningPermissionProvider>();
    }
}
