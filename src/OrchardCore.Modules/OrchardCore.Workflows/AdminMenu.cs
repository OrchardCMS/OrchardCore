using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.Workflows
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder.Add(T["Workflows"], "5", contentDefinition => contentDefinition
                .AddClass("workflows").Id("workflows")
                .LinkToFirstChild(true)
                    .Add(T["Definitions"], "1", contentItems => contentItems
                        .Action("Index", "WorkflowDefinition", new { area = "OrchardCore.Workflows" })
                        .Permission(Permissions.ManageWorkflows)
                        .LocalNav()
                    ));
        }
    }
}
