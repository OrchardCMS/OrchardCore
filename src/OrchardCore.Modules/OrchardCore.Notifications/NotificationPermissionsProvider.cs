using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                Name = "Administrator",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = "Editor",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = "Moderator",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = "Author",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = "Contributor",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            },
            new PermissionStereotype {
                Name = "Authenticated",
                Permissions = new[] { NotificationPermissions.ManageNotifications }
            }
        };
    }
}
