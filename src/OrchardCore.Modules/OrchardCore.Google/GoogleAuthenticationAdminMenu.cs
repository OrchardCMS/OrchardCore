using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleAuthenticationAdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAuthentication },
    };

    internal readonly IStringLocalizer S;

    public GoogleAuthenticationAdminMenu(IStringLocalizer<GoogleAuthenticationAdminMenu> localizer)
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
                .Add(S["Authentication"], authentication => authentication
                .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                    .AddClass("google")
                    .Id("google")
                    .Action("Index", "Admin", _routeValues)
                    .Permission(Permissions.ManageGoogleAuthentication)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}

public sealed class GoogleAnalyticsAdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAnalytics },
    };

    internal readonly IStringLocalizer S;

    public GoogleAnalyticsAdminMenu(IStringLocalizer<GoogleAnalyticsAdminMenu> localizer)
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
                    .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), google => google
                        .AddClass("googleAnalytics").Id("googleAnalytics")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGoogleAnalytics)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}

public sealed class GoogleTagManagerAdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleTagManager },
    };

    internal readonly IStringLocalizer S;

    public GoogleTagManagerAdminMenu(IStringLocalizer<GoogleTagManagerAdminMenu> localizer)
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
                    .Add(S["Google Tag Manager"], S["Google Tag Manager"].PrefixPosition(), google => google
                        .AddClass("googleTagManager")
                        .Id("googleTagManager")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGoogleTagManager)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
