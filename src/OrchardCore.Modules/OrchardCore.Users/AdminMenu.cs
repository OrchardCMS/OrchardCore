using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LoginSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                .AddClass("security")
                .Id("security")
                .Add(S["Users"], S["Users"].PrefixPosition(), users => users
                    .AddClass("users")
                    .Id("users")
                    .Action("Index", "Admin", UserConstants.Features.Users)
                    .Permission(CommonPermissions.ListUsers)
                    .Resource(new User())
                    .LocalNav()
                )
                .Add(S["Settings"], settings => settings
                    .Add(S["User Login"], S["User Login"].PrefixPosition(), login => login
                        .Permission(CommonPermissions.ManageUsers)
                        .Action("Index", "Admin", _routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
