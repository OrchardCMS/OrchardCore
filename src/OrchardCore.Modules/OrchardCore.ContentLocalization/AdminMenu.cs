using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.ContentLocalization;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _providersRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ContentRequestCultureProviderSettingsDriver.GroupId },
    };

    private static readonly RouteValueDictionary _pickerRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ContentCulturePickerSettingsDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Localization"], localization => localization
                        .Add(S["Content Request Culture Provider"], S["Content Request Culture Provider"].PrefixPosition(), provider => provider
                            .AddClass("contentrequestcultureprovider")
                            .Id("contentrequestcultureprovider")
                            .Action("Index", "Admin", _providersRouteValues)
                            .Permission(Permissions.ManageContentCulturePicker)
                            .LocalNav()
                        )
                        .Add(S["Content Culture Picker"], S["Content Culture Picker"].PrefixPosition(), picker => picker
                            .AddClass("contentculturepicker")
                            .Id("contentculturepicker")
                            .Action("Index", "Admin", _pickerRouteValues)
                            .Permission(Permissions.ManageContentCulturePicker)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
