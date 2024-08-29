using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

public sealed class AdminMenuSignin : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Signin },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuSignin(IStringLocalizer<AdminMenuSignin> localizer)
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
                .Add(S["Sign in with X (Twitter)"], S["Sign in with X (Twitter)"].PrefixPosition(), twitter => twitter
                    .AddClass("twitter")
                    .Id("twitter")
                    .Action("Index", "Admin", _routeValues)
                    .Permission(Permissions.ManageTwitterSignin)
                    .LocalNav())
                )
            );

        return Task.CompletedTask;
    }
}

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Twitter },
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
                    .Add(S["X (Twitter)"], S["X (Twitter)"].PrefixPosition(), twitter => twitter
                        .AddClass("twitter").Id("twitter")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageTwitter)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
