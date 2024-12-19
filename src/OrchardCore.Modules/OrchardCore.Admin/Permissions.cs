using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public sealed class Permissions : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'AdminPermissions.AccessAdminPanel'.")]
    public static readonly Permission AccessAdminPanel = AdminPermissions.AccessAdminPanel;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        AdminPermissions.AccessAdminPanel,
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
