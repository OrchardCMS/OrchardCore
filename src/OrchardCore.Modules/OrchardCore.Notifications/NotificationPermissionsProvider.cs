using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public sealed class NotificationPermissionsProvider : IPermissionProvider
{
    [Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Notifications.NotificationPermissions.ManageNotifications'.")]
    public static readonly Permission ManageNotifications = NotificationPermissions.ManageNotifications;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        NotificationPermissions.ManageNotifications,
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
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Authenticated,
            Permissions = _allPermissions,
        },
    ];
}
