using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using System;

namespace OrchardCore.Templates
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
                .Add(T["Configuration"], content => content
                    .Add(T["Templates"], "10", import => import
                        .Action("Index", "Template", new { area = "OrchardCore.Templates" })
                        .Permission(Permissions.ManageTemplates)
                        .LocalNav()
                    )
                );
        }
    }
}
