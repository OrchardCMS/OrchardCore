using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
    public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
    public static readonly Permission ManageOwnUserInformation = CommonPermissions.EditOwnUser;
    public static readonly Permission EditOwnUser = CommonPermissions.EditOwnUser;
    public static readonly Permission ListUsers = CommonPermissions.ListUsers;
    public static readonly Permission EditUsers = CommonPermissions.EditUsers;
    public static readonly Permission DeleteUsers = CommonPermissions.DeleteUsers;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageUsers,
        ViewUsers,
        EditOwnUser,
        ListUsers,
        EditUsers,
        DeleteUsers,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        EditOwnUser,
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
        }
    ];
}
