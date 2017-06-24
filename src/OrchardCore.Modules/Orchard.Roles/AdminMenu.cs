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
				.Add(T["Design"], design => design
					.Add(T["Security"], "5", security => security
						.Add(T["Roles"], "10", installed => installed
							.Action("Index", "Admin", "Orchard.Roles")
							.Permission(Permissions.ManageRoles)
							.LocalNav()
						)));
        }
    }
}
