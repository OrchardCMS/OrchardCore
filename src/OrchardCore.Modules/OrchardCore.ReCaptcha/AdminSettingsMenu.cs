using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Drivers;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha;

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
                .Add(S["reCaptcha"], S["reCaptcha"].PrefixPosition(), reCaptcha => reCaptcha
                    .Action(GetRouteValues(ReCaptchaSettingsDisplayDriver.GroupId))
                    .Permission(Permissions.ManageReCaptchaSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
