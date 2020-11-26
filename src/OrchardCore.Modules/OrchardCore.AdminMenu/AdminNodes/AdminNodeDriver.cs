using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Models;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Security.Services;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class AdminNodeDriver<TConcrete> : DisplayDriver<MenuItem, TConcrete>
    where TConcrete : AdminNode
    {

        private readonly IPermissionsService _permissionService;

        public AdminNodeDriver(
            IPermissionsService permissionService)
        {
            _permissionService = permissionService;
        }


        protected async Task InitializePermissionPicker(IPermissionPickerViewModel model, TConcrete treeNode)
        {

            model.SelectedItems = new List<VueMultiselectItemViewModel>();
            model.AllItems = new List<VueMultiselectItemViewModel>();

            var nameList = new List<string>();
            var allInstalledPermissions = await _permissionService.GetInstalledPermissionsAsync();

            //add to selected permissions only the ones that are enabled or unchanged
            foreach (var savedPermission in treeNode.Permissions)
            {
                var realPermission = allInstalledPermissions.Where(p => p.Name == savedPermission.Name).FirstOrDefault();
                if (realPermission != null)
                {
                    nameList.Add(realPermission.Name);

                    model.SelectedItems.Add(new VueMultiselectItemViewModel
                    {
                        Id = realPermission.Name,
                        DisplayText = $"{realPermission.Name} - {realPermission.Description}"
                    });
                }
            }

            model.PermissionIds = String.Join(",", nameList);

            foreach (var permission in allInstalledPermissions)
            {
                model.AllItems.Add(new VueMultiselectItemViewModel
                {
                    Id = permission.Name,
                    DisplayText = $"{permission.Name} - {permission.Description}"
                });
            }
        }

        protected async Task SavePermissionPicked(IPermissionPickerViewModel model, TConcrete treeNode)
        {
            var modifiedPermissions = (model.PermissionIds == null ? new string[0] : model.PermissionIds.Split(',', StringSplitOptions.RemoveEmptyEntries));
            //clear the old permissions to insert all every time
            treeNode.Permissions.Clear();

            //change permissions only if one is inserted
            if (modifiedPermissions.Length > 0)
            {
                var permissions = await _permissionService.GetInstalledPermissionsAsync();

                foreach (var permissionName in modifiedPermissions)
                {
                    var perm = permissions.Where(p => p.Name == permissionName).FirstOrDefault();

                    if (perm != null)
                    {
                        treeNode.Permissions.Add(perm);
                    }
                }
            }
        }
    }
}