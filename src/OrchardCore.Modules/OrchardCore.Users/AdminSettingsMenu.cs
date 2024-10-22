using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

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
                .Add(S["User Login"], S["User Login"].PrefixPosition(), login => login
                    .Permission(CommonPermissions.ManageUsers)
                    .Action(GetRouteValues(LoginSettingsDisplayDriver.GroupId))
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
