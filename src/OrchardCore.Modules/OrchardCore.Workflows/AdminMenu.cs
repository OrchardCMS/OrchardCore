using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Workflows
{
    public class AdminMenu : INavigationProvider
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Workflows"], NavigationConstants.AdminMenuWorkflowsPosition, workflow => workflow
                .AddClass("workflows").Id("workflows").Action("Index", "WorkflowType", new { area = "OrchardCore.Workflows" })
                    .Permission(Permissions.ManageWorkflows)
                    .LocalNav());

            return Task.CompletedTask;
        }
    }
}
