using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook;

public class AdminMenu : INavigationProvider
{
#pragma warning disable IDE1006 // Naming Styles
    private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

    public AdminMenu(
        IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Meta App"], S["Meta App"].PrefixPosition(), facebook => facebook
                        .AddClass("facebookApp")
                        .Id("facebookApp")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Core })
                        .Permission(Permissions.ManageFacebookApp)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}

public class AdminMenuLogin : INavigationProvider
{
#pragma warning disable IDE1006 // Naming Styles
    private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

    public AdminMenuLogin(
        IStringLocalizer<AdminMenuLogin> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Authentication"], authentication => authentication
                    .Add(S["Meta"], S["Meta"].PrefixPosition(), settings => settings
                        .AddClass("facebook")
                        .Id("facebook")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Login })
                        .Permission(Permissions.ManageFacebookApp)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}

public class AdminMenuPixel : INavigationProvider
{
#pragma warning disable IDE1006 // Naming Styles
    private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

    public AdminMenuPixel(
        IStringLocalizer<AdminMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Meta Pixel"], S["Meta Pixel"].PrefixPosition(), pixel => pixel
                        .AddClass("facebookPixel")
                        .Id("facebookPixel")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.PixelSettingsGroupId })
                        .Permission(FacebookConstants.ManageFacebookPixelPermission)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
