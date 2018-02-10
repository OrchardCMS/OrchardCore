using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using System;

namespace OrchardCore.OpenId
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
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Security"], "5", security => security
                        .Add(T["OpenID Connect Apps"], "15", installed => installed
                            .Action("Index", "Admin", "OrchardCore.OpenId")
                            .Permission(Permissions.ManageOpenIdApplications)
                            .LocalNav()
                        ))
                    .Add(T["Settings"], settings => settings
                        .Add(T["OpenID Connect"], "10", entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "open id" })
                            .LocalNav()
                        ))
                );
        }
    }
}
