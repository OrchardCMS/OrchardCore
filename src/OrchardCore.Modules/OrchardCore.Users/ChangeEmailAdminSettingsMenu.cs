using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class ChangeEmailAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ChangeEmailAdminSettingsMenu(IStringLocalizer<ChangeEmailAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["User Change email"], S["User Change email"].PrefixPosition(), email => email
                    .Permission(CommonPermissions.ManageUsers)
                    .Action(GetRouteValues(ChangeEmailSettingsDisplayDriver.GroupId))
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
