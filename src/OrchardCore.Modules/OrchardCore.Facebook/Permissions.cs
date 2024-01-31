using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageFacebookApp
            = new("ManageFacebookApp", "View and edit the Facebook app.");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageFacebookApp }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageFacebookApp,
                }
            };
        }
    }
}
