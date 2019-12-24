using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCultures = new Permission("ManageCultures", "Manage supported culture");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageCultures }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCultures }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageCultures }
                }
            };
        }
    }
}
