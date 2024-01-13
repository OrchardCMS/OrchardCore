using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
           => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageIndexes,
    ];
}
