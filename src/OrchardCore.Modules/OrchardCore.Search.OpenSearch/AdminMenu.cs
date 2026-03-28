using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Search.OpenSearch.Core.Models;

namespace OrchardCore.Search.OpenSearch;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly OpenSearchConnectionOptions _connectionOptions;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IOptions<OpenSearchConnectionOptions> connectionOptions,
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
                    .Add(S["Run OpenSearch Query"], S["Run OpenSearch Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Search.OpenSearch")
                        .AddClass("opensearchquery")
                        .Id("opensearchquery")
                        .Permission(OpenSearchPermissions.ManageOpenSearchIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
