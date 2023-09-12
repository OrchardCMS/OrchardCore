using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security
{
    public class SecurityPermissions : IPermissionProvider
    {
        public static readonly Permission ManageSecurityHeadersSettings = new("ManageSecurityHeadersSettings", "Manage Security Headers Settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult(new[] { ManageSecurityHeadersSettings }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            => new[]
                {
                    new PermissionStereotype
                    {
                        Name = "Administrator",
                        Permissions = new[] { ManageSecurityHeadersSettings },
                    },
                };
    }
}
