using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Drivers;

namespace OrchardCore.RateLimits;

internal sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", RateLimitsSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                        .Add(S["Rate Limits"], S["Rate Limits"].PrefixPosition(), rateLimits => rateLimits
                            .Action("Index", "Admin", _routeValues)
                            .Permission(RateLimitsPermissions.ManageRateLimits)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Rate Limits"], S["Rate Limits"].PrefixPosition(), rateLimits => rateLimits
                        .Action("Index", "Admin", _routeValues)
                        .Permission(RateLimitsPermissions.ManageRateLimits)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
