using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users
{
    public class UserRolePermissions : IPermissionProvider
    {
        private readonly IRoleService _roleService;

        public UserRolePermissions(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
                .Select(role => CommonPermissions.CreatePermissionForManageUsersInRole(role));
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => Enumerable.Empty<PermissionStereotype>();
    }
}
