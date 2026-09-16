using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class ChangeEmailAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ChangeEmailSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public ChangeEmailAdminMenu(IStringLocalizer<ChangeEmailAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["User Change Email"], S["User Change Email"].PrefixPosition(), email => email
                            .Permission(UsersPermissions.ManageUsers)
                            .Action("Index", "Admin", s_routeValues)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Change Email"], S["Change Email"].PrefixPosition(), email => email
                        .Permission(UsersPermissions.ManageUsers)
                        .Action("Index", "Admin", s_routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
