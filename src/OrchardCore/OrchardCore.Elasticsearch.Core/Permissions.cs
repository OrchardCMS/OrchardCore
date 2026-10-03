using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class Permissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", new LocalizationSource("Manage Elasticsearch Indexes", typeof(Permissions)));

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", new LocalizationSource("Query Elasticsearch Api", typeof(Permissions)), [ManageElasticIndexes]);
}
