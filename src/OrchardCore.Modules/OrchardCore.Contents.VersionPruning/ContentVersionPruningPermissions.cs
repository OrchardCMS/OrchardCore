using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.VersionPruning;

public static class ContentVersionPruningPermissions
{
    public static readonly Permission ManageContentVersionPruningSettings = new(
        "ManageContentVersionPruningSettings",
        "Manage Content Version Pruning settings");
}
