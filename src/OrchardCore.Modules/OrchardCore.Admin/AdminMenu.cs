using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Admin;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", AdminSiteSettingsDisplayDriver.GroupId },
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
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Admin"], S["Admin"].PrefixPosition(), admin => admin
                        .AddClass("admin").Id("admin")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(PermissionsAdminSettings.ManageAdminSettings)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
