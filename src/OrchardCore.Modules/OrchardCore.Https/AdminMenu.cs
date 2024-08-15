using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Https.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Https;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", HttpsSettingsDisplayDriver.GroupId },
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
            .Add(S["Security"], security => security
                .Add(S["Settings"], settings => settings
                    .Add(S["HTTPS"], S["HTTPS"].PrefixPosition(), https => https
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageHttps)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
