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
            return Task.FromResult(_permissions);
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = _permissions,
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = _permissions,
                },
                new PermissionStereotype
                {
                    Name = "Moderator",
                    Permissions = _permissions,
                },
                new PermissionStereotype
                {
                    Name = "Author",
                    Permissions = _permissions,
                },
                new PermissionStereotype
                {
                    Name = "Contributor",
                    Permissions = _permissions,
                }
            };
        }

        private readonly IEnumerable<Permission> _permissions =
        [
            AccessAdminPanel
        ];
    }
}
