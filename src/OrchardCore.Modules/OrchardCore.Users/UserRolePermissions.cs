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
        public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
        public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
        public static readonly Permission EditOwnUserInformation = new("ManageOwnUserInformation", "Edit own user information", new Permission[] { ManageUsers });

        private readonly IRoleService _roleService;

        public UserRolePermissions(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>()
            {
                CommonPermissions.AssignRole
            };

            var roles = (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x).ToList();

            foreach (var role in roles)
            {
                list.Add(CommonPermissions.CreateAssignUserToRolePermission(role));
            }

            return list;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        CommonPermissions.AssignRole,
                    }
                }
            };
        }
    }
}

