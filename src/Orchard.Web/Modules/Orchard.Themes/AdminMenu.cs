using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Themes
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer<AdminMenu> T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Themes"], "10", menu => menu.Action("Index", "Admin", new { area = "Orchard.Themes" }).Permission(Permissions.ApplyTheme)
                    .Add(T["Installed"], "0", item => item.Action("Index", "Admin", new { area = "Orchard.Themes" }).Permission(Permissions.ApplyTheme)));
        }
    }
}
