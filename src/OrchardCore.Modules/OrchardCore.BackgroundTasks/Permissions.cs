using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageBackgroundTasks = new("ManageBackgroundTasks", "Manage background tasks");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageBackgroundTasks },
                }
            };
        }

        private static IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageBackgroundTasks };
        }
    }
}
