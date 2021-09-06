using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
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
                .Add(S["Content"], content => content
                    .AddClass("media").Id("media")
                    .Add(S["Media Library"], S["Media Library"].PrefixPosition(), media => media
                        .Permission(Permissions.ManageMedia)
                        .Action("Index", "Admin", new { area = "OrchardCore.Media" })
                        .LocalNav()
                    ));

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Media Options"], S["Media Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", new { area = "OrchardCore.Media" })
                        .Permission(Permissions.ViewMediaOptions)
                        .LocalNav())
                    .Add(S["Media Profiles"], S["Media Profiles"].PrefixPosition(), mediaProfiles => mediaProfiles
                        .Action("Index", "MediaProfiles", new { area = "OrchardCore.Media" })
                        .Permission(Permissions.ManageMediaProfiles)
                        .LocalNav())
            ));

            return Task.CompletedTask;
        }
    }

    public class MediaCacheAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public MediaCacheAdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Media Cache"], S["Media Cache"].PrefixPosition(), cache => cache
                        .Action("Index", "MediaCache", new { area = "OrchardCore.Media" })
                        .Permission(MediaCachePermissions.ManageAssetCache)
                        .LocalNav())
            ));

            return Task.CompletedTask;
        }
    }
}
