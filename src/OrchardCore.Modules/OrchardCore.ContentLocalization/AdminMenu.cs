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
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], localization => localization
                    .Add(S["Settings"], settings => settings
                        .Add(S["Localization"], localization => localization
                            .Add(S["Content Request Culture Provider"], S["Content Request Culture Provider"].PrefixPosition(), registration => registration
                                .AddClass("contentrequestcultureprovider").Id("contentrequestcultureprovider")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentRequestCultureProviderSettingsDriver.GroupId })
                                .Permission(Permissions.ManageContentCulturePicker)
                                .LocalNav()
                            )
                            .Add(S["Content Culture Picker"], S["Content Culture Picker"].PrefixPosition(), registration => registration
                                .AddClass("contentculturepicker").Id("contentculturepicker")
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ContentCulturePickerSettingsDriver.GroupId })
                                .Permission(Permissions.ManageContentCulturePicker)
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
