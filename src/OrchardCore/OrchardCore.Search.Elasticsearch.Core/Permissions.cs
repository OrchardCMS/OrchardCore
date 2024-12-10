using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch;

public static class Permissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", "Manage Elasticsearch Indexes");

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", "Query Elasticsearch Api", [ManageElasticIndexes]);
}
