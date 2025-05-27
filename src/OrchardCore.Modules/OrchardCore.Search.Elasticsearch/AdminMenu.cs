using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Elasticsearch;

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
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Elasticsearch Query"], S["Run Elasticsearch Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Search.Elasticsearch")
                        .AddClass("elasticsearchquery")
                        .Id("elasticsearchquery")
                        .Permission(ElasticsearchPermissions.ManageElasticIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
