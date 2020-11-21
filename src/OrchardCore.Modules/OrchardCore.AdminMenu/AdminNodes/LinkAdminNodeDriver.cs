using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Security.Services;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeDriver : DisplayDriver<MenuItem, LinkAdminNode>
    {

        private readonly IPermissionsService _permissionService;

        public LinkAdminNodeDriver(
            IPermissionsService permissionService)
        {
            _permissionService = permissionService;
        }
        public override IDisplayResult Display(LinkAdminNode treeNode)
        {
            return Combine(
                View("LinkAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("LinkAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(LinkAdminNode treeNode)
        {
            return Initialize<LinkAdminNodeViewModel>("LinkAdminNode_Fields_TreeEdit", async model =>
            {
                model.LinkText = treeNode.LinkText;
                model.LinkUrl = treeNode.LinkUrl;
                model.IconClass = treeNode.IconClass;
                model.SelectedItems = new List<VueMultiselectItemViewModel>();
                model.AllItems = new List<VueMultiselectItemViewModel>();

                var nameList = new List<string>();

                foreach (var permission in treeNode.Permissions)
                {
                    nameList.Add(permission.Name);

                    model.SelectedItems.Add(new VueMultiselectItemViewModel
                    {
                        Id = permission.Name,
                        DisplayText = $"{permission.Name} - {permission.Description}"
                    });
                }

                model.PermissionIds = String.Join(",", nameList);

                foreach (var permission in await _permissionService.GetInstalledPermissionsAsync())
                {
                    model.AllItems.Add(new VueMultiselectItemViewModel
                    {
                        Id = permission.Name,
                        DisplayText = $"{permission.Name} - {permission.Description}"
                    });
                }

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkAdminNode treeNode, IUpdateModel updater)
        {
            var model = new LinkAdminNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkUrl, x => x.LinkText, x => x.IconClass, x => x.PermissionIds))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.LinkUrl = model.LinkUrl;
                treeNode.IconClass = model.IconClass;

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
            };

            return Edit(treeNode);
        }
    }
}
