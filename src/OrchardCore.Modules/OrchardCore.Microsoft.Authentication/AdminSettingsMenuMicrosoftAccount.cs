using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication;

public sealed class AdminSettingsMenuMicrosoftAccount : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuMicrosoftAccount(IStringLocalizer<AdminSettingsMenuMicrosoftAccount> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["Microsoft"], S["Microsoft"].PrefixPosition(), microsoft => microsoft
                    .AddClass("microsoft")
                    .Id("microsoft")
                    .Action(GetRouteValues(MicrosoftAuthenticationConstants.Features.MicrosoftAccount))
                    .Permission(Permissions.ManageMicrosoftAuthentication)
                    .LocalNav()
                )
           );

        return ValueTask.CompletedTask;
    }
}
