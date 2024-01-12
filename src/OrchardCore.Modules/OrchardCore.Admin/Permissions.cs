using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Infrastructure.Security;
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
                    Name = RoleNames.Administrator,
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Editor,
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Moderator,
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Author,
                    Permissions = GetPermissions(),
                },
                new PermissionStereotype
                {
                    Name = RoleNames.Contributor,
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
