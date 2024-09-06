using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class ResetPasswordAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ResetPasswordSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public ResetPasswordAdminMenu(IStringLocalizer<ResetPasswordAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["Settings"], settings => settings
                    .Add(S["User Reset password"], S["User Reset password"].PrefixPosition(), password => password
                        .Permission(CommonPermissions.ManageUsers)
                        .Action("Index", "Admin", _routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
