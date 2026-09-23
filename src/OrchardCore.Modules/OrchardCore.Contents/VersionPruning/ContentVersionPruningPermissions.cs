using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.VersionPruning;

public static class ContentVersionPruningPermissions
{
    public static readonly Permission ManageContentVersionPruningSettings = new(
        "ManageContentVersionPruningSettings",
        LocalizedString.Create("Manage Content Version Pruning settings", typeof(ContentVersionPruningPermissions)));
}
