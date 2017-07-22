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
                .Add(T["Design"], design => design
                    .Add(T["Security"], "5", security => security
                        .Add(T["OpenID Connect Apps"], "15", installed => installed
                            .Action("Index", "Admin", "Orchard.OpenId")
                            .Permission(Permissions.ManageOpenIdApplications)
                            .LocalNav()
                        ))
                    .Add(T["Settings"], settings => settings
                        .Add(T["OpenID Connect"], "10", entry => entry
                            .Action("Index", "Admin", new { area = "Orchard.Settings", groupId = "open id" })
                            .LocalNav()
                        ))
                );
        }
    }
}
