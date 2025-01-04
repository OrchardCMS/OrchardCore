using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Demo;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission DemoAPIAccess = new("DemoAPIAccess", "Access to Demo API ");
    public static readonly Permission ManageOwnUserProfile = new("ManageOwnUserProfile", "Manage own user profile", new Permission[] { CommonPermissions.ManageUsers });

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        DemoAPIAccess,
        ManageOwnUserProfile,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageOwnUserProfile,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Authenticated,
            Permissions =
            [
                DemoAPIAccess,
            ],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Moderator,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Author,
            Permissions = _generalPermissions,
        },
    ];
}
