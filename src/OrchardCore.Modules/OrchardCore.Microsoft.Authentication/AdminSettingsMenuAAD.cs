using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication;

public sealed class AdminSettingsMenuAAD : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuAAD(IStringLocalizer<AdminSettingsMenuAAD> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Authentication"], authentication => authentication
                .Add(S["Microsoft Entra ID"], S["Microsoft Entra ID"].PrefixPosition(), entraId => entraId
                    .AddClass("microsoft-entra-id")
                    .Id("microsoft-entra-id")
                    .Action(GetRouteValues(MicrosoftAuthenticationConstants.Features.AAD))
                    .Permission(Permissions.ManageMicrosoftAuthentication)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
