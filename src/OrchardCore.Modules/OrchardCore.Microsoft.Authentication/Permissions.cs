using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageMicrosoftAuthentication
            = new(nameof(ManageMicrosoftAuthentication), "Manage Microsoft Authentication settings");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageMicrosoftAuthentication }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            yield return new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageMicrosoftAuthentication,
                }
            };
        }
    }
}
