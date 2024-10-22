using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class RegistrationAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public RegistrationAdminSettingsMenu(IStringLocalizer<RegistrationAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["User Registration"], S["User Registration"].PrefixPosition(), registration => registration
                    .Action(GetRouteValues(RegistrationSettingsDisplayDriver.GroupId))
                    .Permission(CommonPermissions.ManageUsers)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
