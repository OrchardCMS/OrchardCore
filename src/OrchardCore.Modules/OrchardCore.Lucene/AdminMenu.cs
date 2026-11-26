using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene;

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
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Lucene Query"], S["Run Lucene Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Lucene")
                        .AddClass("lucenequery")
                        .Id("lucenequery")
                        .Permission(LuceneSearchPermissions.ManageLuceneIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
