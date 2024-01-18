using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Lucene;

public class AdminMenu(IStringLocalizer<AdminMenu> localizer) : INavigationProvider
{
    protected readonly IStringLocalizer S = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search").Id("search")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                    .Add(S["Lucene Indices"], S["Lucene Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", new { area = "OrchardCore.Search.Lucene" })
                        .Permission(Permissions.ManageLuceneIndexes)
                        .LocalNav()
                     )
                )
            .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Lucene Query"], S["Run Lucene Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", new { area = "OrchardCore.Search.Lucene" })
                        .Permission(Permissions.ManageLuceneIndexes)
                        .LocalNav()
                        )
                    )
                );

        return Task.CompletedTask;
    }
}
