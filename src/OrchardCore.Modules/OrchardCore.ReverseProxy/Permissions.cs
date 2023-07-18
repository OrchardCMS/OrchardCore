using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ReverseProxy
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageReverseProxySettings = new("ManageReverseProxySettings", "Manage Reverse Proxy Settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageReverseProxySettings }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageReverseProxySettings },
                },
            };
        }
    }
}
