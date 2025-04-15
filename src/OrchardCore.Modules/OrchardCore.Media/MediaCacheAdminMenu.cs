using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class MediaCacheAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

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
                        .Add(S["Media Cache"], S["Media Cache"].PrefixPosition(), cache => cache
                            .Action("Index", "MediaCache", "OrchardCore.Media")
                            .Permission(MediaPermissions.ManageAssetCache)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Cache"], S["Cache"].PrefixPosition(), cache => cache
                        .Action("Index", "MediaCache", "OrchardCore.Media")
                        .Permission(MediaPermissions.ManageAssetCache)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
