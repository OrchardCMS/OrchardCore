using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.ContentLocalization
{
    [Feature("OrchardCore.ContentLocalization.ContentCulturePicker")]
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], localization => localization
                    .Add(T["Settings"], settings => settings
                        .Add(T["Content Culture Picker"], T["Content Culture Picker"], registration => registration
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentCulturePickerSettingsDriver.GroupId })
                            .Permission(Permissions.ManageContentCulturePicker)
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
