using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Layers
{
    public class AdminLayerPermissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminLayers = new Permission("ManageAdminLayers", "Manage admin layers");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageAdminLayers }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminLayers }
                }
            };
        }
    }    
}
