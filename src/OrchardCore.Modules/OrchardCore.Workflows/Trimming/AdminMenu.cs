using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.Trimming.Drivers;

namespace OrchardCore.Workflows.Trimming;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", WorkflowTrimmingDisplayDriver.GroupId },
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
                .Add(S["Workflow Trimming"], S["Workflow Trimming"], trimming => trimming
                    .Action("Index", "Admin", _routeValues)
                    .Permission(Permissions.ManageWorkflowSettings)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}
