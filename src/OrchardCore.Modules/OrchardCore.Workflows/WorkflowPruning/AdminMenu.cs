using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.WorkflowPruning.Drivers;

namespace OrchardCore.Workflows.WorkflowPruning;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder.Add(
            S["Configuration"],
            configuration =>
                configuration.Add(
                    S["Settings"],
                    settings =>
                        settings.Add(
                            S["Workflow Pruning"],
                            S["Workflow Pruning"],
                            pruning =>
                                pruning
                                    .Action(
                                        "Index",
                                        "Admin",
                                        new
                                        {
                                            area = "OrchardCore.Settings",
                                            groupid = WorkflowPruningDisplayDriver.GroupId
                                        }
                                    )
                                    .Permission(Permissions.ManageWorkflowSettings)
                                    .LocalNav()
                        )
                )
        );

        return Task.CompletedTask;
    }
}
