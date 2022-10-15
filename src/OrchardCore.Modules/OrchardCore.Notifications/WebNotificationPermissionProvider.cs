using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class WebNotificationPermissionProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[]
        {
                WebNotificationPermission.ManageOwnNotifications,
            }
        .AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            },
            new PermissionStereotype {
                Name = "Editor",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            },
            new PermissionStereotype {
                Name = "Moderator",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            },
            new PermissionStereotype {
                Name = "Author",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            },
            new PermissionStereotype {
                Name = "Contributor",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            },
            new PermissionStereotype {
                Name = "Authenticated",
                Permissions = new[] { WebNotificationPermission.ManageOwnNotifications }
            }
        };
    }
}
