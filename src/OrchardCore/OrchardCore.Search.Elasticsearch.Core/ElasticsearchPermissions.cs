using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public static class ElasticsearchPermissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", "Manage Elasticsearch Indexes");

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", "Query Elasticsearch Api", [ManageElasticIndexes]);

}
