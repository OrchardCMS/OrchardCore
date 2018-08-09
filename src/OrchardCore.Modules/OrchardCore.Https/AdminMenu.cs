using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Https
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
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Settings"], settings => settings
                        .Add(T["HTTPS"], "100", entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "Https" })
                            .LocalNav()
                        ))
                );
        }
    }
}
