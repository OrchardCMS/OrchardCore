using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using System;

namespace OrchardCore.Users
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
						.Add(T["Users"], "5", installed => installed
							.Action("Index", "Admin", "OrchardCore.Users")
							.Permission(Permissions.ManageUsers)
							.LocalNav()
						)));
        }
    }
}
