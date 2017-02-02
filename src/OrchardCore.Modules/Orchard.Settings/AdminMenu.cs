using System;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;

namespace Orchard.Settings
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
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder.Add(T["Design"], design => design
                .Add(T["Settings"], "1", settings => settings
                    .Add(T["General"], T["General"], entry => entry
                        .Action("Index", "Admin", new { area = "Orchard.Settings", groupId = "general" })
                        .LocalNav()
                    )
                )
            );

            builder.Add(T["Design"], design => design
                .Add(T["Site"], "1", site => site
                    .Add(T["Restart"], "99", settings => settings
                        .Action("RestartSite", "Admin", new { area = "Orchard.Settings" })
                        .LocalNav()
                    )
                )
            );
        }
    }
}