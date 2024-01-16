using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class NotificationPermissionsProvider : IPermissionProvider
{
    public static readonly Permission ManageNotifications = NotificationPermissions.ManageNotifications;

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageNotifications,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Moderator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Authenticated",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
