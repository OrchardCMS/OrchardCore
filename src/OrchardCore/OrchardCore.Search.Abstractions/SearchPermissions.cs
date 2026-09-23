using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public static class SearchPermissions
{
    public static readonly Permission ManageSearchSettings = new("ManageSearchSettings", LocalizedString.Create("Manage Search Settings", typeof(SearchPermissions)));
}
