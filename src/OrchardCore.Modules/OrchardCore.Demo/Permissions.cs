using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Demo;

public class Permissions : IPermissionProvider
{
    public static readonly Permission DemoAPIAccess = new("DemoAPIAccess", "Access to Demo API ");
    public static readonly Permission ManageOwnUserProfile = new("ManageOwnUserProfile", "Manage own user profile", new Permission[] { CommonPermissions.ManageUsers });

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        DemoAPIAccess,
        ManageOwnUserProfile,
    ];

    private static readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageOwnUserProfile,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Authenticated",
            Permissions =
            [
                DemoAPIAccess,
            ],
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Moderator",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = _generalPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
