using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.WorkflowPruning.Drivers;

namespace OrchardCore.Workflows.WorkflowPruning;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        // Since features admin accepts tenant, always pass empty string to create valid link for current tenant.
        { "groupId", WorkflowPruningDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder.Add(S["Configuration"], configuration => configuration
            .Add(S["Settings"], settings => settings
                .Add(S["Workflow Pruning"], S["Workflow Pruning"], pruning => pruning
                    .Action("Index","Admin", _routeValues)
                    .Permission(Permissions.ManageWorkflowSettings)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}
