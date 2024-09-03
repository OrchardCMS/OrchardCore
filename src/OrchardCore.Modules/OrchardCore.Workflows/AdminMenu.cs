using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Workflows;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Workflows"], NavigationConstants.AdminMenuWorkflowsPosition, workflow => workflow
                .AddClass("workflows")
                .Id("workflows")
                .Action("Index", "WorkflowType", "OrchardCore.Workflows")
                .Permission(Permissions.ManageWorkflows)
                .LocalNav()
            );

        return ValueTask.CompletedTask;
    }
}
