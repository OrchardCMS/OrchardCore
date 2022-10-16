using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class WebNotificationPermissionsProvider : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[]
        {
                WebNotificationPermissions.ManageWebNotifications,
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
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            },
            new PermissionStereotype {
                Name = "Editor",
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            },
            new PermissionStereotype {
                Name = "Moderator",
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            },
            new PermissionStereotype {
                Name = "Author",
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            },
            new PermissionStereotype {
                Name = "Contributor",
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            },
            new PermissionStereotype {
                Name = "Authenticated",
                Permissions = new[] { WebNotificationPermissions.ManageWebNotifications }
            }
        };
    }
}
