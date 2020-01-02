using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Localization
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer<AdminMenu> S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder
                    .Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, localization => localization
                    .AddClass("localization").Id("localization")
                        .Add(S["Settings"], settings => settings
                            .Add(S["Cultures"], S["Cultures"], entry => entry
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LocalizationSettingsDisplayDriver.GroupId })
                                .Permission(Permissions.ManageCultures)
                                .LocalNav()
                            )
                        )
                    );
            }

            return Task.CompletedTask;
        }
    }
}
