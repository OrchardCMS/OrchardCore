using System;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;

namespace Orchard.Media
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
                .Add(T["Content"], design => design
                    .Add(T["Media"], "3", layers => layers
                        .Action("Index", "Admin", new { area = "Orchard.Media" })
                        .LocalNav()
                    ));
        }
    }
}
