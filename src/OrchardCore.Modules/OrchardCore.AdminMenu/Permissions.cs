using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", "Manage the admin menu");

        public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", "View Admin Menu - View All", new[] { ManageAdminMenu });

        private static readonly Permission _viewAdminMenu = new("ViewAdminMenu_{0}", "View Admin Menu - {0}", new[] { ManageAdminMenu, ViewAdminMenuAll });

        private readonly IAdminMenuService _adminMenuService;

        public Permissions(IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission> { ManageAdminMenu, ViewAdminMenuAll };

            foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
            {
                list.Add(CreatePermissionForAdminMenu(adminMenu.Name));
            }

            return list;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdminMenu }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageAdminMenu }
                },
            };
        }

        public static Permission CreatePermissionForAdminMenu(string name)
        {
            return new Permission(
                    String.Format(_viewAdminMenu.Name, name),
                    String.Format(_viewAdminMenu.Description, name),
                    _viewAdminMenu.ImpliedBy
                );
        }
    }
}
