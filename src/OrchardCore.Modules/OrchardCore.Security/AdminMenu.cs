using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;

namespace OrchardCore.Security;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SecuritySettingsDisplayDriver.GroupId },

    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                .AddClass("security")
                .Id("security")
                .Add(S["Settings"], settings => settings
                    .Add(S["Security Headers"], S["Security Headers"].PrefixPosition(), headers => headers
                        .Permission(SecurityPermissions.ManageSecurityHeadersSettings)
                        .Action("Index", "Admin", _routeValues)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
