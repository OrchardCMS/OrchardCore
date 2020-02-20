using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Security.Services
{
    public static class RoleServiceExtensions
    {
        public static async Task<IEnumerable<string>> GetRoleNamesAsync(this IRoleService roleService)
        {
            var roles = await roleService.GetRolesAsync();

            return roles.Select(r => r.RoleName);
        }
    }
}
