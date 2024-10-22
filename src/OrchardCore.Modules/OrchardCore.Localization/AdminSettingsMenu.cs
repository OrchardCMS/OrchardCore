using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a localization menu in the admin site.
/// </summary>
public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    /// <summary>
    /// Creates a new instance of the <see cref="AdminSettingsMenu"/>.
    /// </summary>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    /// <inheritdocs />
    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Localization"], localization => localization
                .AddClass("localization")
                .Id("localization")
                .Add(S["Cultures"], S["Cultures"].PrefixPosition(), cultures => cultures
                    .AddClass("cultures")
                    .Id("cultures")
                    .Action(GetRouteValues(LocalizationSettingsDisplayDriver.GroupId))
                    .Permission(Permissions.ManageCultures)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
