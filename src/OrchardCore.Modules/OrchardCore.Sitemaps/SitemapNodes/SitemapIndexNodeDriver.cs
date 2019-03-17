using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNodeDriver : DisplayDriver<SitemapNode, SitemapIndexNode>
    {
        public override IDisplayResult Display(SitemapIndexNode treeNode)
        {
            return Combine(
                View("SitemapIndexNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("SitemapIndexNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(SitemapIndexNode treeNode)
        {
            return Initialize<SitemapIndexNodeViewModel>("SitemapIndexNode_Fields_TreeEdit", model =>
            {
                model.Description = treeNode.Description;
                model.Path = treeNode.Path;
                model.SitemapNode = treeNode;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapIndexNode treeNode, IUpdateModel updater)
        {
            var model = new SitemapIndexNodeViewModel();
            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.Description, x => x.Path))
            {
                treeNode.Description = model.Description;
                treeNode.Path = model.Path;
            };

            return Edit(treeNode);
        }
    }
}
