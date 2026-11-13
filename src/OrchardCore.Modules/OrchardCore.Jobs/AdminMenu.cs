using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Jobs;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Jobs"], S["Jobs"].PrefixPosition(), jobs => jobs
                    .Add(S["Runs"], S["Runs"].PrefixPosition("1"), runs => runs
                        .Action("Index", "JobRun", "OrchardCore.Jobs")
                        .Permission(JobsPermissions.ViewJobs)
                        .LocalNav()
                    )
                    .Add(S["Jobs"], S["Jobs"].PrefixPosition("2"), list => list
                        .Action("Index", "Job", "OrchardCore.Jobs")
                        .Permission(JobsPermissions.ViewJobs)
                        .LocalNav()
                    )
                    .Add(S["Schedules"], S["Schedules"].PrefixPosition("3"), schedules => schedules
                        .Action("Index", "JobSchedule", "OrchardCore.Jobs")
                        .Permission(JobsPermissions.ViewJobs)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
