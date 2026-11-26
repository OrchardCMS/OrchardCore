using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

public sealed class MediaCacheAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public MediaCacheAdminMenu(IStringLocalizer<MediaCacheAdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
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
