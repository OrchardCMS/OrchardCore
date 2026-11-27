using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a localization menu in the admin site.
/// </summary>
public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LocalizationSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    /// <summary>
    /// Creates a new instance of the <see cref="AdminMenu"/>.
    /// </summary>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    /// <inheritdocs />
    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Localization"], S["Localization"].PrefixPosition(), localization => localization
                    .AddClass("localization")
                    .Id("localization")
                    .Add(S["Cultures"], S["Cultures"].PrefixPosition(), cultures => cultures
                        .AddClass("cultures")
                        .Id("cultures")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(LocalizationPermissions.ManageCultures)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
