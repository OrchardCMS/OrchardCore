using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Localization"], localization => localization
                .Add(S["Content Request Culture Provider"], S["Content Request Culture Provider"].PrefixPosition(), provider => provider
                    .AddClass("contentrequestcultureprovider")
                    .Id("contentrequestcultureprovider")
                    .Action(GetRouteValues(ContentRequestCultureProviderSettingsDriver.GroupId))
                    .Permission(Permissions.ManageContentCulturePicker)
                    .LocalNav()
                )
                .Add(S["Content Culture Picker"], S["Content Culture Picker"].PrefixPosition(), picker => picker
                    .AddClass("contentculturepicker")
                    .Id("contentculturepicker")
                    .Action(GetRouteValues(ContentCulturePickerSettingsDriver.GroupId))
                    .Permission(Permissions.ManageContentCulturePicker)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
