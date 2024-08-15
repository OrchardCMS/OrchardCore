using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.Features.Core },
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
                    .Add(S["Meta App"], S["Meta App"].PrefixPosition(), metaApp => metaApp
                        .AddClass("facebookApp")
                        .Id("facebookApp")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageFacebookApp)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}

public sealed class AdminMenuLogin : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.Features.Login },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuLogin(
        IStringLocalizer<AdminMenuLogin> localizer)
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
                    .Add(S["Meta"], S["Meta"].PrefixPosition(), meta => meta
                        .AddClass("facebook")
                        .Id("facebook")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageFacebookApp)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}

public sealed class AdminMenuPixel : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.PixelSettingsGroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuPixel(
        IStringLocalizer<AdminMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
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
                    .Add(S["Meta Pixel"], S["Meta Pixel"].PrefixPosition(), pixel => pixel
                        .AddClass("facebookPixel")
                        .Id("facebookPixel")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(FacebookConstants.ManageFacebookPixelPermission)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
