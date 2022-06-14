using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tenants
{
    public class FeaturePermissions : IPermissionProvider
    {
        public static readonly Permission ManageTenantFeatures = new Permission("ManageTenantFeatures", "Manage Features for a tenant");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageTenantFeatures }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ManageTenantFeatures
                    }
                }
            };
        }
    }
}
