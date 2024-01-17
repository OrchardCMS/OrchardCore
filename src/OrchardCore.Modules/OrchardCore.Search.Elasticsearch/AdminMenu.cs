using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Elasticsearch;

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
                .AddClass("elasticsearch").Id("Elasticsearch")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                    .Add(S["Elasticsearch Indices"], S["Elasticsearch Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", new { area = "OrchardCore.Search.Elasticsearch" })
                        .Permission(Permissions.ManageElasticIndexes)
                        .LocalNav()
                        )
                    )
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Elasticsearch Query"], S["Run Elasticsearch Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", new { area = "OrchardCore.Search.Elasticsearch" })
                        .Permission(Permissions.ManageElasticIndexes)
                        .LocalNav()
                        )
                    )
                );

        return Task.CompletedTask;
    }
}
