using OrchardCore.Indexing.Core;
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class ElasticsearchPermissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", LocalizationSource.Create("Manage Elasticsearch Indexes", typeof(ElasticsearchPermissions)), [IndexingPermissions.ManageIndexes]);

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", LocalizationSource.Create("Query Elasticsearch Api", typeof(ElasticsearchPermissions)), [ManageElasticIndexes]);
}
