using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = CommonPermissions.ManageUsers;
        public static readonly Permission ViewUsers = CommonPermissions.ViewUsers;
        public static readonly Permission EditOwnUserInformation = new("ManageOwnUserInformation", "Edit own user information", new Permission[] { ManageUsers });

        private readonly IServiceProvider _serviceProvider;

        private IRoleService _roleService;

        public Permissions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>()
            {
                EditOwnUserInformation
            };

            // lazy resolve the RoleService and it may not be available since the Users modules does not have direct dependeny on the Roles module
            _roleService ??= _serviceProvider.GetService<IRoleService>();

            if (_roleService != null)
            {
                var roles = (await _roleService.GetRoleNamesAsync())
                    .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x).ToList();

                list.Add(CommonPermissions.ListUsers);
                foreach (var role in roles)
                {
                    list.Add(CommonPermissions.CreateListUsersInRolePermission(role));
                }

                list.Add(CommonPermissions.EditUsers);
                foreach (var role in roles)
                {
                    list.Add(CommonPermissions.CreateEditUsersInRolePermission(role));
                }

                list.Add(CommonPermissions.DeleteUsers);
                foreach (var role in roles)
                {
                    list.Add(CommonPermissions.CreateDeleteUsersInRolePermission(role));
                }
            }

            return list;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        ManageUsers,
                        CommonPermissions.ListUsers,
                        CommonPermissions.EditUsers,
                        CommonPermissions.DeleteUsers,
                        CommonPermissions.ManageUserProfileSettings
                    }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { EditOwnUserInformation }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { EditOwnUserInformation }
                }
            };
        }
    }
}
