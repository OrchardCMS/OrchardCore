using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class PermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        Permissions.AccessAdminPanel,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Moderator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Author,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions = _allPermissions,
        },
    ];
}
