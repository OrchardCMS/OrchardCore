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
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Localization"], localization => localization
                        .Add(S["Content request culture provider"], S["Content request culture provider"].PrefixPosition(), provider => provider
                            .AddClass("contentrequestcultureprovider")
                            .Id("contentrequestcultureprovider")
                            .Action("Index", "Admin", _providersRouteValues)
                            .Permission(Permissions.ManageContentCulturePicker)
                            .LocalNav()
                        )
                        .Add(S["Content culture picker"], S["Content culture picker"].PrefixPosition(), picker => picker
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

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Localization"], S["Localization"].PrefixPosition(), localization => localization
                    .Add(S["Content culture"], S["Content culture"].PrefixPosition(), provider => provider
                        .AddClass("contentrequestcultureprovider")
                        .Id("contentrequestcultureprovider")
                        .Action("Index", "Admin", _providersRouteValues)
                        .Permission(Permissions.ManageContentCulturePicker)
                        .LocalNav()
                    )
                    .Add(S["Content culture picker"], S["Content culture picker"].PrefixPosition(), picker => picker
                        .AddClass("contentculturepicker")
                        .Id("contentculturepicker")
                        .Action("Index", "Admin", _pickerRouteValues)
                        .Permission(Permissions.ManageContentCulturePicker)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
