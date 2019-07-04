using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminMenu = new Permission("ManageAdminMenu", "Manage the admin menu");

        public static readonly Permission ViewAdminMenuAll = new Permission("ViewAdminMenuAll", "View Admin Menu - View All", new[] { ManageAdminMenu });

        private static readonly Permission ViewAdminMenu = new Permission("ViewAdminMenu_{0}", "View Admin Menu - {0}", new[] { ManageAdminMenu, ViewAdminMenuAll });

        private readonly IAdminMenuService _adminMenuService;

        public Permissions(IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }


        public IEnumerable<Permission> GetPermissions()
        {
            var list = new List<Permission> { ManageAdminMenu, ViewAdminMenuAll };

            foreach (var adminMenu in _adminMenuService.GetAsync().GetAwaiter().GetResult())
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
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageAdminMenu }
                }
            };
        }

        public static Permission CreatePermissionForAdminMenu(string name)
        {
            return new Permission(
                    String.Format(ViewAdminMenu.Name, name),
                    String.Format(ViewAdminMenu.Description, name),
                    ViewAdminMenu.ImpliedBy
                );
        }
    }
}