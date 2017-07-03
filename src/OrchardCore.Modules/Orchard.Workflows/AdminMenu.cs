using System;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;

namespace Orchard.Workflows {
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
                    .Add(T["Definition"], "1", contentItems => contentItems
                        .Action("Index", "Admin", new { area = "Orchard.Workflows" })
                        .Permission(Permissions.ManageWorkflows)
                        .LocalNav()
                    ));
        }
    }
}
