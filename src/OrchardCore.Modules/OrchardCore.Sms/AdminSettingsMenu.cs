using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Sms;

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
            .Add(S["Communication"], S["Communication"].PrefixPosition(), communication => communication
                .Add(S["SMS"], S["SMS"].PrefixPosition(), sms => sms
                    .AddClass("sms")
                    .Id("sms")
                    .Action(GetRouteValues(SmsSettings.GroupId))
                    .Permission(SmsPermissions.ManageSmsSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
