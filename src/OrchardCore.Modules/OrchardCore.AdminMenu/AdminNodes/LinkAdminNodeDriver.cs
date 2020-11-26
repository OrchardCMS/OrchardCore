using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeDriver : AdminNodeDriver<LinkAdminNode>
    {
        public LinkAdminNodeDriver(IPermissionsService permissionService) : base(permissionService)
        {
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
                
                await InitializePermissionPicker(model, treeNode);

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

                await SavePermissionPicked(model, treeNode);
            };

            return Edit(treeNode);
        }
    }
}
