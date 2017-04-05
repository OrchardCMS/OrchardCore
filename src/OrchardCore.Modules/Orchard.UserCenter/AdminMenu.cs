using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.UserCenter
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
				.Add(T["UserCenter"], design => design
                    .Add(T["UserCenter"], "5", installed => installed
                            .Action("Index", "UserCenter", "Orchard.UserCenter")
                            .Permission(Permissions.AccessUserCenterPanel)
                            ));
        }
    }
}
