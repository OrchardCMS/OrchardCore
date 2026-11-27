using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Provides admin menu entries for the Data Localization module.
/// </summary>
public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.DataLocalization" },
        { "controller", "Admin" },
        { "action", "Index" },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Localization"], S["Localization"].PrefixPosition(), localization => localization
                    .Add(S["Dynamic Translations"], S["Dynamic Translations"].PrefixPosition(), translations => translations
                        .AddClass("dynamic-translations")
                        .Id("dynamicTranslations")
                        .Permission(DataLocalizationPermissions.ViewDynamicTranslations)
                        .Action(s_routeValues["action"].ToString(), s_routeValues["controller"].ToString(), s_routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
