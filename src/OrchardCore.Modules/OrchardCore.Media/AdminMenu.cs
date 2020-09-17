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
                    .Add(S["Media Library"], S["Media Library"].PrefixPosition(), layers => layers
                        .Permission(Permissions.ManageOwnMedia)
                        .Action("Index", "Admin", new { area = "OrchardCore.Media" })
                        .LocalNav()
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

            builder.Add(S["Content"], content => content
                .Add(S["Media Cache"], S["Media Cache"].PrefixPosition(), contentItems => contentItems
                    .Action("Index", "MediaCache", new { area = "OrchardCore.Media" })
                    .Permission(MediaCachePermissions.ManageAssetCache)
                    .LocalNav())
                );

            return Task.CompletedTask;
        }
    }
}
