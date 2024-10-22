using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Google;

public sealed class GoogleAuthenticationAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public GoogleAuthenticationAdminSettingsMenu(IStringLocalizer<GoogleAuthenticationAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                    .AddClass("google")
                    .Id("google")
                    .Action(GetRouteValues(GoogleConstants.Features.GoogleAuthentication))
                    .Permission(Permissions.ManageGoogleAuthentication)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
