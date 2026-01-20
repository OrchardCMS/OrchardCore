using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageUsers = UsersPermissions.ManageUsers;
    public static readonly Permission ViewUsers = UsersPermissions.ViewUsers;
    public static readonly Permission ManageOwnUserInformation = UsersPermissions.EditOwnUser;
    public static readonly Permission EditOwnUser = UsersPermissions.EditOwnUser;
    public static readonly Permission ListUsers = UsersPermissions.ListUsers;
    public static readonly Permission EditUsers = UsersPermissions.EditUsers;
    public static readonly Permission DeleteUsers = UsersPermissions.DeleteUsers;

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
