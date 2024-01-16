using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public class Permissions : IPermissionProvider
{
    public static readonly Permission QuerySearchIndex = new("QuerySearchIndex", "Query any index");

    public static readonly Permission ManageSearchSettings = new("ManageSearchSettings", "Manage Search Settings");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        QuerySearchIndex,
        ManageSearchSettings,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
