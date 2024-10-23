using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Drivers;

namespace OrchardCore.Workflows.Trimming;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Site"], site => site
                .Add(S["Workflow Trimming"], S["Workflow Trimming"], trimming => trimming
                    .Action(GetRouteValues(WorkflowTrimmingDisplayDriver.GroupId))
                    .Permission(Permissions.ManageWorkflowSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
