using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Elasticsearch;

public static class Permissions
{
    public static readonly Permission ManageElasticIndexes = new("ManageElasticIndexes", LocalizedString.Create("Manage Elasticsearch Indexes", typeof(Permissions)));

    public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", LocalizedString.Create("Query Elasticsearch Api", typeof(Permissions)), [ManageElasticIndexes]);
}
