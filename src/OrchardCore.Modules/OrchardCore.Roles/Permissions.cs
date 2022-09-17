using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageRoles = CommonPermissions.ManageRoles;
        public static readonly Permission AssignRoles = CommonPermissions.AssignRoles;

        private readonly IRoleService _roleService;
        private readonly IPermissionLocalizer _permissionLocalizer;

        public Permissions(IRoleService roleService, IPermissionLocalizer permissionLocalizer)
        {
            _roleService = roleService;
            _permissionLocalizer = permissionLocalizer;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>
            {
                ManageRoles,
                AssignRoles,
                StandardPermissions.SiteOwner
            };

            var roles = (await _roleService.GetRoleNamesAsync())
                .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);

            foreach (var role in roles)
            {
                list.Add(CommonPermissions.CreatePermissionForAssignRole(role));
            }

            return list.Select(permission => _permissionLocalizer.Localizer(permission));
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageRoles, StandardPermissions.SiteOwner }
                },
            };
        }
    }
}
