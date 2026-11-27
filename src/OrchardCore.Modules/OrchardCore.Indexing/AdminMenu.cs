using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Core;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Indexing;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Search), NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexes"], S["Indexes"].PrefixPosition(), indexes => indexes
                    .Action("Index", "Admin", "OrchardCore.Indexing")
                    .AddClass("indexes")
                    .Id("indexes")
                    .Permission(IndexingPermissions.ManageIndexes)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
