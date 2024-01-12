using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Https
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageHttps = new("ManageHttps", "Manage HTTPS");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageHttps }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = RoleNames.Administrator,
                    Permissions = new[] { ManageHttps },
                }
            };
        }
    }
}
