using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
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

    private static readonly RouteValueDictionary _statisticsRouteValues = new()
    {
        { "area", "OrchardCore.DataLocalization" },
        { "controller", "Admin" },
        { "action", "Statistics" },
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
                                .Permission(Permissions.ViewTranslations)
                                .Action(_routeValues["action"].ToString(), _routeValues["controller"].ToString(), _routeValues)
                                .LocalNav()
                            )
                            .Add(S["Translation Statistics"], S["Translation Statistics"].PrefixPosition(), statistics => statistics
                                .AddClass("translationstatistics")
                                .Id("translationstatistics")
                                .Permission(Permissions.ViewTranslations)
                                .Action(_statisticsRouteValues["action"].ToString(), _statisticsRouteValues["controller"].ToString(), _statisticsRouteValues)
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
                    .Add(S["Translations"], S["Translations"].PrefixPosition(), translations => translations
                        .AddClass("translations")
                        .Id("translations")
                        .Permission(Permissions.ViewTranslations)
                        .Action(_routeValues["action"].ToString(), _routeValues["controller"].ToString(), _routeValues)
                        .LocalNav()
                    )
                    .Add(S["Translation Statistics"], S["Translation Statistics"].PrefixPosition(), statistics => statistics
                        .AddClass("translationstatistics")
                        .Id("translationstatistics")
                        .Permission(Permissions.ViewTranslations)
                        .Action(_statisticsRouteValues["action"].ToString(), _statisticsRouteValues["controller"].ToString(), _statisticsRouteValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
