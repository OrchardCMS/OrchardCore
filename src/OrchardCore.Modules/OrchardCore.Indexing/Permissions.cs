using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Indexing
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageIndexes = new("ManageIndexes", "Manage Indexes");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        private static IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ManageIndexes,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ManageIndexes },
                },
            };
        }
    }
}
