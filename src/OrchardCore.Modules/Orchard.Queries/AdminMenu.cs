using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Queries
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

            builder.Add(T["Content"], content => content
                .Add(T["Queries"], "1", contentItems => contentItems
                    .Action("Index", "Admin", new { area = "Orchard.Queries" })
                    .Permission(Permissions.ManageQueries)
                    .LocalNav())
                );            
        }
    }
}
