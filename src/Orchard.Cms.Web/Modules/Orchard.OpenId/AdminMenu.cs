using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.OpenId
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

            builder
                .Add(T["Content"], content => content
                    .Add(T["OpenId Apps"], "5", installed => installed
                        .Action("Index", "Admin", "Orchard.OpenId")
                        .Permission(Permissions.ManageOpenIdApplications)
                        .LocalNav()
                    )
                )
                .Add(T["Design"], design => design
                    .Add(T["Settings"], settings => settings
                        .Add(T["Open Id"], "10", entry => entry
                            .Action("Index", "Admin", new { area = "Orchard.Settings", groupId = "open id" })
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
