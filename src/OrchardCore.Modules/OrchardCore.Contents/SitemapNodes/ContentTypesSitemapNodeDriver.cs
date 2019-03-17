using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNodeDriver : DisplayDriver<SitemapNode, ContentTypesSitemapNode>
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
                ChangeFrequency = treeNode.ContentTypes.Where(selected => selected.ContentTypeId == x.Name).FirstOrDefault()?.ChangeFrequency ?? ChangeFrequency.Daily,
                Priority = treeNode.ContentTypes.Where(selected => selected.ContentTypeId == x.Name).FirstOrDefault()?.IndexPriority ?? 0.5f
            }).ToArray();


            return Initialize<ContentTypesSitemapNodeViewModel>("ContentTypesSitemapNode_Fields_TreeEdit", model =>
            {
                model.Description = treeNode.Description;
                model.Path = treeNode.Path;
                model.IndexAll = treeNode.IndexAll;
                model.ChangeFrequency = treeNode.ChangeFrequency;
                model.Priority = treeNode.Priority;
                model.ContentTypes = entries;
                model.SitemapNode = treeNode;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemapNode treeNode, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            treeNode.ContentTypes = Array.Empty<ContentTypeSitemapEntry>();

            var model = new ContentTypesSitemapNodeViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.Description, x => x.Path, x => x.IndexAll, x => x.ChangeFrequency, x => x.Priority, x => x.ContentTypes))
            {
                treeNode.Description = model.Description;
                treeNode.Path = model.Path;
                treeNode.IndexAll = model.IndexAll;
                treeNode.ChangeFrequency = model.ChangeFrequency;
                treeNode.Priority = model.Priority;
                treeNode.ContentTypes = model.ContentTypes
                    .Where(x => x.IsChecked == true)
                    .Select(x => new ContentTypeSitemapEntry { ContentTypeId = x.ContentTypeId, ChangeFrequency = x.ChangeFrequency, IndexPriority = x.Priority })
                    .ToArray();
            };

            return Edit(treeNode);
        }
    }
}
