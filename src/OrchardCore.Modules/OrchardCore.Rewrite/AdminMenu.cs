using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Rewrite.Drivers;

namespace OrchardCore.Rewrite;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", RewriteSettingsDisplayDriver.GroupId },
    };

    public readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                   .Add(S["Rewrite"], S["Rewrite"].PrefixPosition(), seo => seo
                       .Permission(Permissions.ManageRewrites)
                       .Action("Index", "Admin", _routeValues)
                       .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
