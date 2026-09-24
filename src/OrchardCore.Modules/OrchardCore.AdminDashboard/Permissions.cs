using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageAdminDashboard = new("ManageAdminDashboard", LocalizedString.Create("Manage the Admin Dashboard", typeof(Permissions)));
    public static readonly Permission AccessAdminDashboard = new("AccessAdminDashboard", LocalizedString.Create("Access the Admin Dashboard", typeof(Permissions)), new[] { ManageAdminDashboard });

    private readonly IEnumerable<Permission> _allPermissions =
    [
        AccessAdminDashboard,
        ManageAdminDashboard,
    ];

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        AccessAdminDashboard,
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
            Name = OrchardCoreConstants.Roles.Author,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Contributor,
            Permissions = _generalPermissions,
        },
    ];
}
