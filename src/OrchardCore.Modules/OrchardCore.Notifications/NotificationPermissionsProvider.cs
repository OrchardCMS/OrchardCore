using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class NotificationPermissionsProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
        new PermissionStereotype
        {
            Name = "Moderator",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
        new PermissionStereotype
        {
            Name = "Author",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
        new PermissionStereotype
        {
            Name = "Contributor",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
        new PermissionStereotype
        {
            Name = "Authenticated",
            Permissions = new[] { NotificationPermissions.ManageNotifications }
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        NotificationPermissions.ManageNotifications,
    ];
}
