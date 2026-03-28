using OrchardCore.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenSearch;

public static class OpenSearchPermissions
{
    public static readonly Permission ManageOpenSearchIndexes = new("ManageOpenSearchIndexes", "Manage OpenSearch Indexes", [IndexingPermissions.ManageIndexes]);

    public static readonly Permission QueryOpenSearchApi = new("QueryOpenSearchApi", "Query OpenSearch Api", [ManageOpenSearchIndexes]);
}
