using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Cors
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCorsSettings = new("ManageCorsSettings", "Managing Cors Settings", isSecurityCritical: true);

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageCorsSettings,
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCorsSettings },
                },
            };
        }

    }
}
