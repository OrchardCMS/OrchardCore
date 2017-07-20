using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Templates
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
                .Add(T["Design"], content => content
                    .Add(T["Templates"], "10", import => import
                        .Action("Index", "Template", new { area = "Orchard.Templates" })
                        .Permission(Permissions.ManageTemplates)
                        .LocalNav()
                    )
                );
        }
    }
}
