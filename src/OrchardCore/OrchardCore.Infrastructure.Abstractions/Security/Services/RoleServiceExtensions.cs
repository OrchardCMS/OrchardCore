using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Services
{
    public static class RoleServiceExtensions
    {
        public static async Task<IEnumerable<string>> GetRoleNamesAsync(this IRoleService roleService)
        {
            var roles = await roleService.GetRolesAsync();

            return roles.Select(r => r.RoleName);
        }

        public static async Task<IEnumerable<IRole>> GetAccessibleRolesAsync(this IRoleService roleService, IAuthorizationService authorizationService, ClaimsPrincipal user, Permission permission)
        {
            var roles = await roleService.GetRolesAsync();

            var accessibleRoles = new List<IRole>();
            foreach (var role in roles)
            {
                if (RoleHelper.SystemRoleNames.Contains(role.RoleName))
                {
                    continue;
                }

                if (!await authorizationService.AuthorizeAsync(user, permission, role))
                {
                    continue;
                }

                accessibleRoles.Add(role);
            }

            return accessibleRoles;
        }

        public static async Task<IEnumerable<string>> GetAccessibleRoleNamesAsync(this IRoleService roleService, IAuthorizationService authorizationService, ClaimsPrincipal user, Permission permission)
        {
            var roles = await roleService.GetAccessibleRolesAsync(authorizationService, user, permission);

            return roles.Select(x => x.RoleName);
        }
    }
}
