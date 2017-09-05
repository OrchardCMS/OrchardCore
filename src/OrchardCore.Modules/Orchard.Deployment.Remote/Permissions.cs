using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageRemoteInstances = new Permission("ManageRemoteInstances", "Manage remote instances");
        public static readonly Permission ManageRemoteClients = new Permission("ManageRemoteClients", "Manage remote clients");
        public static readonly Permission Export = new Permission("ExportRemoteInstances", "Export to remote instances");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageRemoteInstances, ManageRemoteClients, Export };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageRemoteInstances, ManageRemoteClients, Export }
                }
            };
        }
    }
}