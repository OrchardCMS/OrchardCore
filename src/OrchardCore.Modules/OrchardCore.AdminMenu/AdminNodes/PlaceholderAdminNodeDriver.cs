using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNodeDriver : DisplayDriver<MenuItem, PlaceholderAdminNode>
    {
        public override IDisplayResult Display(PlaceholderAdminNode treeNode)
        {
            return Combine(
                View("PlaceholderAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("PlaceholderAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(PlaceholderAdminNode treeNode)
        {
            return Initialize<PlaceholderAdminNodeViewModel>("PlaceholderAdminNode_Fields_TreeEdit", model =>
            {
                model.LinkText = treeNode.LinkText;
                model.IconClass = treeNode.IconClass;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(PlaceholderAdminNode treeNode, IUpdateModel updater)
        {
            var model = new PlaceholderAdminNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkText, x => x.IconClass))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.IconClass = model.IconClass;
            };

            return Edit(treeNode);
        }
    }
}
