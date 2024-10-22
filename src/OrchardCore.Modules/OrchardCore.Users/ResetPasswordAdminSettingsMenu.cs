using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class ResetPasswordAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ResetPasswordAdminSettingsMenu(IStringLocalizer<ResetPasswordAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["User Reset password"], S["User Reset password"].PrefixPosition(), password => password
                    .Action(GetRouteValues(ResetPasswordSettingsDisplayDriver.GroupId))
                    .Permission(CommonPermissions.ManageUsers)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
