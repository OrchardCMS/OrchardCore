using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
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
            // TODO Prefer IsRoutable() to reduce list size, and allow SelectAll
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            var entries = contentTypeDefinitions.Select(x => new ContentTypeSitemapEntryViewModel
            {
                ContentTypeName = x.Name,
                IsChecked = treeNode.ContentTypes.Any(selected => selected.ContentTypeName == x.Name),
                ChangeFrequency = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.ChangeFrequency ?? ChangeFrequency.Daily,
                Priority = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Priority ?? 0.5f,
                TakeAll = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.TakeAll ?? true,
                Skip = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Skip ?? 0,
                Take = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Take ?? 50000,
            }).ToArray();


            return Initialize<ContentTypesSitemapNodeViewModel>("ContentTypesSitemapNode_Fields_TreeEdit", model =>
            {
                model.Description = treeNode.Description;
                model.Path = treeNode.Path;
                model.ContentTypes = entries;
                model.SitemapNode = treeNode;
                model.CanSupportChildNodes = treeNode.CanSupportChildNodes;
                model.CanBeChildNode = treeNode.CanBeChildNode;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesSitemapNode treeNode, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            treeNode.ContentTypes = Array.Empty<ContentTypeSitemapEntry>();

            var model = new ContentTypesSitemapNodeViewModel();

            if (await updater.TryUpdateModelAsync(model,
                Prefix,
                m => m.Description,
                m => m.Path,
                m => m.ContentTypes))
            {
                treeNode.Description = model.Description;
                treeNode.Path = model.Path;
                treeNode.ContentTypes = model.ContentTypes
                    .Where(x => x.IsChecked == true)
                    .Select(x => new ContentTypeSitemapEntry
                    {
                        ContentTypeName = x.ContentTypeName,
                        ChangeFrequency = x.ChangeFrequency,
                        Priority = x.Priority,
                        TakeAll = x.TakeAll,
                        Skip = x.Skip,
                        Take = x.Take
                    })
                    .ToArray();
            };

            return Edit(treeNode);
        }
    }
}
