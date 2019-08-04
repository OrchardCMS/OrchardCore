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
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Settings"], settings => settings
                        .Add(T["ContentCulturePicker"], T["ContentCulturePicker"], registration => registration
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentCulturePickerSettingsDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
