using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a localization menu in the admin site.
/// </summary>
public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LocalizationSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    /// <summary>
    /// Creates a new instance of the <see cref="AdminMenu"/>.
    /// </summary>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    /// <inheritdocs />
    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Localization"], localization => localization
                        .AddClass("localization")
                        .Id("localization")
                        .Add(S["Cultures"], S["Cultures"].PrefixPosition(), cultures => cultures
                            .AddClass("cultures")
                            .Id("cultures")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageCultures)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
