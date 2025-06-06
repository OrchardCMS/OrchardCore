using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public static class SearchPermissions
{
    public static readonly Permission ManageSearchSettings = new("ManageSearchSettings", "Manage Search Settings");
}
