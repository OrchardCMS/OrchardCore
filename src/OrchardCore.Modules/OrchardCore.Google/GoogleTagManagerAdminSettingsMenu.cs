using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Google;

public sealed class GoogleTagManagerAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public GoogleTagManagerAdminSettingsMenu(IStringLocalizer<GoogleTagManagerAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                .Id("googleSettings")
                .Add(S["Tag Manager"], S["Tag Manager"].PrefixPosition(), tagManager => tagManager
                    .AddClass("googleTagManager")
                    .Id("googleTagManager")
                    .Action(GetRouteValues(GoogleConstants.Features.GoogleTagManager))
                    .Permission(Permissions.ManageGoogleTagManager)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
