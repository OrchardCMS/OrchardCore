using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNodeDriver : DisplayDriver<MenuItem, ContentTypesSitemapNode>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypesSitemapNodeDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }
        public override IDisplayResult Display(ContentTypesSitemapNode treeNode)
        {
            return Combine(
                View("ContentTypesSitemapNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("ContentTypesSitemapNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(ContentTypesSitemapNode treeNode)
        {
            var listable = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                .OrderBy(ctd => ctd.DisplayName).ToList();


            var entries = listable.Select(x => new ContentTypeSitemapEntryViewModel
            {
                ContentTypeId = x.Name,
                IsChecked = treeNode.ContentTypes.Any(selected => String.Equals(selected.ContentTypeId, x.Name, StringComparison.OrdinalIgnoreCase)),
                IconClass = treeNode.ContentTypes.Where(selected => selected.ContentTypeId == x.Name).FirstOrDefault()?.IconClass ?? String.Empty
            }).ToArray();


            return Initialize<ContentTypesSitemapNodeViewModel>("ContentTypesSitemapNode_Fields_TreeEdit", model =>
            {
                model.ShowAll = treeNode.ShowAll;
                model.IconClass = treeNode.IconClass;
                model.ContentTypes = entries;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemapNode treeNode, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            treeNode.ContentTypes = Array.Empty<ContentTypeSitemapEntry>();

            var model = new ContentTypesSitemapNodeViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.ShowAll, x => x.IconClass, x => x.ContentTypes)) {

                treeNode.ShowAll = model.ShowAll;
                treeNode.IconClass = model.IconClass;
                treeNode.ContentTypes = model.ContentTypes
                    .Where( x => x.IsChecked == true)
                    .Select(x => new ContentTypeSitemapEntry { ContentTypeId = x.ContentTypeId, IconClass = x.IconClass })                    
                    .ToArray();
            };

            return Edit(treeNode);
        }
    }
}
