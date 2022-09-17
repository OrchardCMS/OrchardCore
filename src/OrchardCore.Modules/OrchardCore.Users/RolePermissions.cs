using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users
{
    public class RolePermissions : IPermissionProvider
    {
        private readonly IRoleService _roleService;

        public RolePermissions(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>();

            var roles = (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);

            foreach (var role in roles)
            {
                list.Add(CommonPermissions.CreatePermissionForManageUsersInRole(role));
            }

            return list;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => Enumerable.Empty<PermissionStereotype>();
    }
}
