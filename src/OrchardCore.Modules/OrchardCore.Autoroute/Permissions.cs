using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");
        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissions(),
                },
            };
        }

        private static IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                SetHomepage,
            };
        }
    }
}
