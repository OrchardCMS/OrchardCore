using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageBackgroundTasks = new Permission("ManageBackgroundTasks", "Manage background tasks");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageBackgroundTasks };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageBackgroundTasks }
                }
            };
        }
    }
}