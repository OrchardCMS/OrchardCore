using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users;

public sealed class RegistrationAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", RegistrationSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public RegistrationAdminMenu(IStringLocalizer<RegistrationAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Registration"], S["Registration"].PrefixPosition(), registration => registration
                        .Permission(UsersPermissions.ManageUsers)
                        .Action("Index", "Admin", s_routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
