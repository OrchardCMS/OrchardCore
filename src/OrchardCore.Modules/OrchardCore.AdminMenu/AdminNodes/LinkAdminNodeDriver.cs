using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNodeDriver : DisplayDriver<MenuItem, LinkAdminNode>
    {
        public override IDisplayResult Display(LinkAdminNode treeNode)
        {
            return Combine(
                View("LinkAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("LinkAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(LinkAdminNode treeNode)
        {
            return Initialize<LinkAdminNodeViewModel>("LinkAdminNode_Fields_TreeEdit", model =>
            {
                model.LinkText = treeNode.LinkText;
                model.LinkUrl = treeNode.LinkUrl;
                model.IconClass = treeNode.IconClass;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkAdminNode treeNode, IUpdateModel updater)
        {
            var model = new LinkAdminNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkUrl, x => x.LinkText, x => x.IconClass))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.LinkUrl = model.LinkUrl;
                treeNode.IconClass = model.IconClass;
            };

            return Edit(treeNode);
        }
    }
}
