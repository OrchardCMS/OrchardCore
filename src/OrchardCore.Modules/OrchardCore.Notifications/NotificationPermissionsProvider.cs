using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class NotificationPermissionsProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[]
        {
                NotificationPermissions.ManageNotifications,
            }
        .AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = RoleNames.Administrator,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = RoleNames.Editor,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = RoleNames.Moderator,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = RoleNames.Author,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = RoleNames.Contributor,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = RoleNames.Authenticated,
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            }
        };
    }
}
