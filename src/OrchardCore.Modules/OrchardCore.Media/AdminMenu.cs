using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class AdminMenu : INavigationProvider
{
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
            .Add(S["Content"], content => content
                .AddClass("media")
                .Id("media")
                .Add(S["Media Library"], S["Media Library"].PrefixPosition(), media => media
                    .Permission(Permissions.ManageMedia)
                    .Action("Index", "Admin", "OrchardCore.Media")
                    .LocalNav()
                )
            );

        builder.Add(S["Configuration"], configuration => configuration
            .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Media Options"], S["Media Options"].PrefixPosition(), options => options
                    .Action("Options", "Admin", "OrchardCore.Media")
                    .Permission(Permissions.ViewMediaOptions)
                    .LocalNav()
                )
                .Add(S["Media Profiles"], S["Media Profiles"].PrefixPosition(), mediaProfiles => mediaProfiles
                    .Action("Index", "MediaProfiles", "OrchardCore.Media")
                    .Permission(Permissions.ManageMediaProfiles)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}

public sealed class MediaCacheAdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public MediaCacheAdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder.Add(S["Configuration"], configuration => configuration
            .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Media Cache"], S["Media Cache"].PrefixPosition(), cache => cache
                    .Action("Index", "MediaCache", "OrchardCore.Media")
                    .Permission(MediaCachePermissions.ManageAssetCache)
                    .LocalNav())
        ));

        return Task.CompletedTask;
    }
}
