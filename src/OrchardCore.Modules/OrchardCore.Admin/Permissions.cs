using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AccessAdminPanel = new Permission("AccessAdminPanel", "Access admin panel");

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
                    Name = BuiltInRole.Administrator,
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Editor,
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Moderator,
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Author,
                    Permissions = GetPermissions()
                },
                new PermissionStereotype {
                    Name = BuiltInRole.Contributor,
                    Permissions = GetPermissions()
                }
            };
        }

        private IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel
            };
        }
    }
}
