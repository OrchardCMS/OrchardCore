using Microsoft.Extensions.Localization;
using OrchardCore.Email.Core;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Email;

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
                .Add(S["Email"], S["Email"].PrefixPosition(), email => email
                    .AddClass("email")
                    .Id("email")
                    .Action(GetRouteValues(EmailSettings.GroupId))
                    .Permission(Permissions.ManageEmailSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
