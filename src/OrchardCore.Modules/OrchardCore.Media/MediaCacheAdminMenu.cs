using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class MediaCacheAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public MediaCacheAdminMenu(IStringLocalizer<MediaCacheAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                        .Add(S["Media cache"], S["Media cache"].PrefixPosition(), cache => cache
                            .Action("Index", "MediaCache", "OrchardCore.Media")
                            .Permission(MediaCachePermissions.ManageAssetCache)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Cache"], S["Cache"].PrefixPosition(), cache => cache
                        .Action("Index", "MediaCache", "OrchardCore.Media")
                        .Permission(MediaCachePermissions.ManageAssetCache)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
