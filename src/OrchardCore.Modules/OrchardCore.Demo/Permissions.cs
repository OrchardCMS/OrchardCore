using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Demo;

public class Permissions : IPermissionProvider
{
    public static readonly Permission DemoAPIAccess = new("DemoAPIAccess", "Access to Demo API ");
    public static readonly Permission ManageOwnUserProfile = new("ManageOwnUserProfile", "Manage own user profile", new Permission[] { CommonPermissions.ManageUsers });

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Authenticated",
            Permissions = _allPermissions,
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

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        DemoAPIAccess,
        ManageOwnUserProfile,
    ];

    private readonly static IEnumerable<Permission> _generalPermissions =
    [
        ManageOwnUserProfile,
    ];
}
