using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Google;

public sealed class GoogleAnalyticsAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public GoogleAnalyticsAdminSettingsMenu(IStringLocalizer<GoogleAnalyticsAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), google => google
                    .AddClass("googleAnalytics")
                    .Id("googleAnalytics")
                    .Action(GetRouteValues(GoogleConstants.Features.GoogleAnalytics))
                    .Permission(Permissions.ManageGoogleAnalytics)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
