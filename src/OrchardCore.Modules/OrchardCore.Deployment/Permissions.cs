using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageDeploymentPlan = CommonPermissions.ManageDeploymentPlan;
        public static readonly Permission Export = CommonPermissions.Export;
        public static readonly Permission Import = CommonPermissions.Import;

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageDeploymentPlan, Import, Export }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { Import, Export },
                }
            };
        }
    }
}
