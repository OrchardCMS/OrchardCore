using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.SitemapNodes;
using OrchardCore.Sitemaps.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class SitemapIndexNodeDriver : DisplayDriver<MenuItem, SitemapIndexNode>
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
                model.LinkText = treeNode.LinkText;
                model.IconClass = treeNode.IconClass;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapIndexNode treeNode, IUpdateModel updater)
        {
            var model = new SitemapIndexNodeViewModel();
            if(await updater.TryUpdateModelAsync(model, Prefix, x => x.LinkText, x => x.IconClass))
            {
                treeNode.LinkText = model.LinkText;
                treeNode.IconClass = model.IconClass;
            };
            
            return Edit(treeNode);
        }
    }
}
