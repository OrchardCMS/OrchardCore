using Microsoft.Extensions.Localization;
using OrchardCore.Https.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Https;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["HTTPS"], S["HTTPS"].PrefixPosition(), https => https
                    .Action(GetRouteValues(HttpsSettingsDisplayDriver.GroupId))
                    .Permission(Permissions.ManageHttps)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
