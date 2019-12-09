using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin
{
    public class PermissionsAdminTheme : IPermissionProvider
    {
        public static readonly Permission ManageAdminTheme = new Permission("ManageAdminTheme", "Manage Admin Theme");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageAdminTheme }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminTheme }
                }
            };
        }
    }
}
