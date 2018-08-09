using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Settings
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

            builder.Add(T["Configuration"], configuration => configuration
                .Add(T["Settings"], "1", settings => settings
                    .Add(T["General"], T["General"], entry => entry
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "general" })
                        .LocalNav()
                    )
                )
            );
        }
    }
}