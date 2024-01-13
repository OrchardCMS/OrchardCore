using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
    public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
    public static readonly Permission ManageOwnUserInformation = CommonPermissions.EditOwnUser;

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
        }
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        CommonPermissions.ManageUsers,
        CommonPermissions.ViewUsers,
        CommonPermissions.EditOwnUser,
        CommonPermissions.ListUsers,
        CommonPermissions.EditUsers,
        CommonPermissions.DeleteUsers,
    ];

    private readonly static IEnumerable<Permission> _generalPermissions =
    [
        CommonPermissions.EditOwnUser,
    ];
}
