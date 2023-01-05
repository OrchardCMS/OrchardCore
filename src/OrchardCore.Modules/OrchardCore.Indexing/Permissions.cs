using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new Permission("ManageIndexes", "Manage Indexes");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        private IEnumerable<Permission> GetPermissions()
        {
            return new[]
{
                ManageIndexes
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageIndexes }
                }
            };
        }
    }
}
