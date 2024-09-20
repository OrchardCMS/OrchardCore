using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public static class IndexingPermissions
{
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");
}
