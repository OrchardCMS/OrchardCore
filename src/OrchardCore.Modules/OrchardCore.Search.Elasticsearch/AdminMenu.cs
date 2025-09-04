using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly ElasticsearchConnectionOptions _connectionOptions;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IOptions<ElasticsearchConnectionOptions> connectionOptions,
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        _connectionOptions = connectionOptions.Value;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (!_connectionOptions.ConfigurationExists())
        {
            return ValueTask.CompletedTask;
        }

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
