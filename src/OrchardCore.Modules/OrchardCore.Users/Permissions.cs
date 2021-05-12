using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
        public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;

        public static readonly Permission ManageOwnUserInformation = new Permission("ManageOwnUserInformation", "Manage own user information", new Permission[] { ManageUsers });

        private readonly IRoleService _roleService;

        public Permissions(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>
            {
                ManageUsers,
                ManageOwnUserInformation,
                ViewUsers
            };

            var roles = (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);

            foreach (var role in roles)
            {
                list.Add(CommonPermissions.CreatePermissionForManageUsersInRole(role));
            }

            return list;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageUsers }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { ManageOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { ManageOwnUserInformation }
                }
            };
        }
    }
}
