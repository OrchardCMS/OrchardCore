using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeDriver : DisplayDriver<MenuItem, LinkAdminNode>
    {
        private readonly IAdminMenuPermissionService _adminMenuPermissionService;

        public LinkAdminNodeDriver(IAdminMenuPermissionService adminMenuPermissionService)
        {
            _adminMenuPermissionService = adminMenuPermissionService;
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

                var permissions = await _adminMenuPermissionService.GetPermissionsAsync();

                var selectedPermissions = permissions.Where(p => treeNode.PermissionNames.Contains(p.Name));

                model.SelectedItems = selectedPermissions
                    .Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayText = p.Description
                    }).ToList();
                model.AllItems = permissions
                    .Select(p => new PermissionViewModel
                    {
                        Name = p.Name,
                        DisplayText = p.Description
                    }).ToList();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkAdminNode treeNode, IUpdateModel updater)
        {
            var model = new LinkAdminNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkUrl, x => x.LinkText, x => x.IconClass, x => x.SelectedPermissionNames))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.LinkUrl = model.LinkUrl;
                treeNode.IconClass = model.IconClass;

                var selectedPermissions = (model.SelectedPermissionNames == null ? Array.Empty<string>() : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries));

                var permissions = await _adminMenuPermissionService.GetPermissionsAsync();
                treeNode.PermissionNames = permissions
                    .Where(p => selectedPermissions.Contains(p.Name))
                    .Select(p => p.Name).ToArray();
            }

            return Edit(treeNode);
        }
    }
}
