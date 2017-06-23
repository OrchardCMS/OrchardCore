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

            builder.Add(T["Workflows"], content => content
                .Add(T["Workflows"], "1", contentItems => contentItems
                    .Action("Index", "Admin", new { area = "Orchard.Workflows" })
                    .Permission(Permissions.ManageWorkflows)
                    .LocalNav())
                );
        }
    }
}
