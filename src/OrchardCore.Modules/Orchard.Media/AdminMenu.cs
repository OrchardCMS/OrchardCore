using System;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;

namespace Orchard.Media
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }
        
        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(S["Content"], design => design
                    .Add(S["Files"], "3", layers => layers
                        .Action("Index", "Admin", new { area = "Orchard.Media" })
                        .LocalNav()
                    ));
        }
    }
}
