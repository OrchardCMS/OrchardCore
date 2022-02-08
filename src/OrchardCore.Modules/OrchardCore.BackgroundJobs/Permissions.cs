using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundJobs
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageBackgroundJobs = new Permission("ManageBackgroundJobs", "Manage background jobs");

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
                    Permissions = new[] { ManageBackgroundJobs }
                }
            };
        }

        private IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageBackgroundJobs };
        }
    }
}
