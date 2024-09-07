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
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Media Cache"], S["Media Cache"].PrefixPosition(), cache => cache
                        .Action("Index", "MediaCache", "OrchardCore.Media")
                        .Permission(MediaCachePermissions.ManageAssetCache)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
