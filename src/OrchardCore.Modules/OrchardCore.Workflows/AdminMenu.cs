using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Workflows;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
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

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Workflows"], S["Workflows"].PrefixPosition(), workflow => workflow
                    .AddClass("workflows")
                    .Id("workflows")
                    .Action("Index", "WorkflowType", "OrchardCore.Workflows")
                    .Permission(Permissions.ManageWorkflows)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
