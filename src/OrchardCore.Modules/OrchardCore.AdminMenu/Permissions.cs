using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdminMenu = new Permission("ManageAdminMenu", "Manage the admin menu");

        public static readonly Permission SeeAdminMenuAll = new Permission("SeeAdminMenuAll", "See Admin Menu - See All", new[] { ManageAdminMenu });

        private static readonly Permission SeeAdminMenu = new Permission("SeeAdminMenu_{0}", "See Admin Menu - {0}", new[] { ManageAdminMenu, SeeAdminMenuAll });

        private readonly IAdminMenuService _adminMenuService;

        public Permissions(IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }


        public IEnumerable<Permission> GetPermissions()
        {
            var list = new List<Permission> { ManageAdminMenu, SeeAdminMenuAll };

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
                    String.Format(SeeAdminMenu.Name, name),
                    String.Format(SeeAdminMenu.Description, name),
                    SeeAdminMenu.ImpliedBy
                );
        }
    }
}