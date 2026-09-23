using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Demo;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission DemoAPIAccess = new("DemoAPIAccess", LocalizedString.Create("Access to Demo API ", typeof(Permissions)));
    public static readonly Permission ManageOwnUserProfile = new("ManageOwnUserProfile", LocalizedString.Create("Manage own user profile", typeof(Permissions)), new Permission[] { UsersPermissions.ManageUsers });

    private static readonly IEnumerable<Permission> s_allPermissions =
    [
        DemoAPIAccess,
        ManageOwnUserProfile,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageOwnUserProfile,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(s_allPermissions);

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
