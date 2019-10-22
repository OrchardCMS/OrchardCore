using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Localization
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder
                    .Add(T["Configuration"], "5", localization => localization
                    .AddClass("localization").Id("localization")
                        .Add(T["Settings"], settings => settings
                            .Add(T["Cultures"], T["Cultures"], entry => entry
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
