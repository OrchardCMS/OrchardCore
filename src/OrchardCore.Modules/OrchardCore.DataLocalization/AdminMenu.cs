using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Provides admin menu entries for the Data Localization module.
/// </summary>
public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.DataLocalization" },
        { "controller", "Admin" },
        { "action", "Index" },
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
                            .Add(S["Translations"], S["Translations"].PrefixPosition(), translations => translations
                                .AddClass("translations")
                                .Id("translations")
                                .Permission(DataLocalizationPermissions.ViewDynamicTranslations)
                                .Action(_routeValues["action"].ToString(), _routeValues["controller"].ToString(), _routeValues)
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
                    .Add(S["Dynamic Translations"], S["Dynamic Translations"].PrefixPosition(), translations => translations
                        .AddClass("dynamic-translations")
                        .Id("dynamicTranslations")
                        .Permission(DataLocalizationPermissions.ViewDynamicTranslations)
                        .Action(_routeValues["action"].ToString(), _routeValues["controller"].ToString(), _routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
