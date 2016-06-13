using System;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.Roles.Services
{
    public interface IRoleManager
    {
        Task<RolesDocument> GetRolesAsync();
        void UpdateRoles();
    }

    public static class RoleManagerExtensions
    {
        public static async Task<Role> GetRoleByNameAsync(this IRoleManager roleManager, string name)
        {
            var roles = await roleManager.GetRolesAsync();
            return roles.Roles.FirstOrDefault(x => String.Equals(x.RoleName, name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<Role> CreateRoleAsync(this IRoleManager roleManager, string name)
        {
            var roles = await roleManager.GetRolesAsync();
            var role = new Role() { RoleName = name };
            roles.Roles.Add(role);
            roleManager.UpdateRoles();
            return role;
        }
    }
}
