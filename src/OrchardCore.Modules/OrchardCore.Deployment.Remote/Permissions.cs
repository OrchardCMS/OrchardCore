using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", "Manage remote instances");
        public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", "Manage remote clients");
        public static readonly Permission Export = new("ExportRemoteInstances", "Export to remote instances");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageRemoteInstances, ManageRemoteClients, Export }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageRemoteInstances, ManageRemoteClients, Export },
                }
            };
        }
    }
}
