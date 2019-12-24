using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Cors
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCorsSettings = new Permission("ManageCorsSettings", "Managing Cors Settings", isSecurityCritical: true);

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageCorsSettings, StandardPermissions.SiteOwner
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageCorsSettings, StandardPermissions.SiteOwner }
                },
            };
        }

    }
}
