using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.OpenSearch;

public static class Permissions
{
    public static readonly Permission ManageOpenSearchIndexes = new("ManageOpenSearchIndexes", "Manage OpenSearch Indexes");

    public static readonly Permission QueryOpenSearchApi = new("QueryOpenSearchApi", "Query OpenSearch Api", [ManageOpenSearchIndexes]);
}
