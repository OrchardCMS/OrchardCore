using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Lucene;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                    .Add(S["Lucene Indices"], S["Lucene Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", "OrchardCore.Search.Lucene")
                        .AddClass("luceneindices")
                        .Id("luceneindices")
                        .Permission(Permissions.ManageLuceneIndexes)
                        .LocalNav()
                     )
                )
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Lucene Query"], S["Run Lucene Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Search.Lucene")
                        .AddClass("lucenequery")
                        .Id("lucenequery")
                        .Permission(Permissions.ManageLuceneIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
