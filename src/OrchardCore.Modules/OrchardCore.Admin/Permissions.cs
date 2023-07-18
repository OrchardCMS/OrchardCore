using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");

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
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = GetPermissions(),
                }
            };
        }

        private static IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel,
            };
        }
    }
}
