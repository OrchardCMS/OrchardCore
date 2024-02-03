using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageAdminDashboard = new("ManageAdminDashboard", "Manage the Admin Dashboard");
    public static readonly Permission AccessAdminDashboard = new("AccessAdminDashboard", "Access the Admin Dashboard", new[] { ManageAdminDashboard });

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
            Name = "Author",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _generalPermissions,
        },
    ];
}
