using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Elasticsearch;

public sealed class AdminMenu(IStringLocalizer<AdminMenu> localizer) : INavigationProvider
{
    internal readonly IStringLocalizer S = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                    .Add(S["Elasticsearch Indices"], S["Elasticsearch Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", "OrchardCore.Search.Elasticsearch")
                        .AddClass("elasticsearchindices")
                        .Id("elasticsearchindices")
                        .Permission(Permissions.ManageElasticIndexes)
                        .LocalNav()
                    )
                )
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Elasticsearch Query"], S["Run Elasticsearch Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Search.Elasticsearch")
                        .AddClass("elasticsearchquery")
                        .Id("elasticsearchquery")
                        .Permission(Permissions.ManageElasticIndexes)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
