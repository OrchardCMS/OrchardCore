using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Roles
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
                    .Add(T["Roles"], "5", installed => installed
                        .Action("Index", "Admin", "Orchard.Roles")
                        .Permission(Permissions.ManageRoles)
                        .LocalNav()
                    )
                );
        }
    }
}
