using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageDeploymentPlan = new Permission("ManageDeploymentPlan", "Manage deployment plan");
        public static readonly Permission Export = new Permission("Export", "Export Data");
        public static readonly Permission Import = new Permission("Import", "Import Data", isSecurityCritical: true);

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
                    Permissions = new[] { Import, Export }
                }
            };
        }
    }
}
