using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class Permissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", LocalizationSource.Create("Manage Elasticsearch Indexes", typeof(Permissions)));

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", LocalizationSource.Create("Query Elasticsearch Api", typeof(Permissions)), [ManageElasticIndexes]);
}
