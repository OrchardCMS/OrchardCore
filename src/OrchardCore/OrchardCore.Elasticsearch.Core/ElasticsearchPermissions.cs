using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class ElasticsearchPermissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", LocalizedString.Create("Manage Elasticsearch Indexes", typeof(ElasticsearchPermissions)), [IndexingPermissions.ManageIndexes]);

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", LocalizedString.Create("Query Elasticsearch Api", typeof(ElasticsearchPermissions)), [ManageElasticIndexes]);
}
