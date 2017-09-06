using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageDeploymentPlan = new Permission("ManageDeploymentPlan", "Manage deployment plan");
        public static readonly Permission Export = new Permission("Export", "Export Data");
        public static readonly Permission Import = new Permission("Import", "Import Data");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageDeploymentPlan, Import, Export };
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