using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.Trimming.Drivers;

namespace OrchardCore.Workflows.Trimming;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", WorkflowTrimmingDisplayDriver.GroupId },
    };

    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
               .Add(S["Configuration"], configuration => configuration
                   .Add(S["Settings"], settings => settings
                       .Add(S["Workflow Trimming"], S["Workflow Trimming"], trimming => trimming
                           .Action("Index", "Admin", _routeValues)
                           .Permission(WorkflowsPermissions.ManageWorkflowSettings)
                           .LocalNav()
                       )
                   )
               );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Workflow Trimming"], S["Workflow Trimming"].PrefixPosition(), trimming => trimming
                    .Action("Index", "Admin", _routeValues)
                    .Permission(WorkflowsPermissions.ManageWorkflowSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
