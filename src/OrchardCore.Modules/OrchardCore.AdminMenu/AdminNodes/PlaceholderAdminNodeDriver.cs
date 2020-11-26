using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNodeDriver : AdminNodeDriver<PlaceholderAdminNode>
    {
        public PlaceholderAdminNodeDriver(IPermissionsService permissionService) : base(permissionService)
        {
        }
        public override IDisplayResult Display(PlaceholderAdminNode treeNode)
        {
            return Combine(
                View("PlaceholderAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("PlaceholderAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(PlaceholderAdminNode treeNode)
        {
            return Initialize<PlaceholderAdminNodeViewModel>("PlaceholderAdminNode_Fields_TreeEdit", async model =>
            {
                model.LinkText = treeNode.LinkText;
                model.IconClass = treeNode.IconClass;
                
                await InitializePermissionPicker(model, treeNode);

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(PlaceholderAdminNode treeNode, IUpdateModel updater)
        {
            var model = new PlaceholderAdminNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkText, x => x.IconClass, x => x.PermissionIds))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.IconClass = model.IconClass;

                await SavePermissionPicked(model, treeNode);

            };

            return Edit(treeNode);
        }
    }
}
