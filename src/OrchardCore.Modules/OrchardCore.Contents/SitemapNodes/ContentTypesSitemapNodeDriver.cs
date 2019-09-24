using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
            //TODO this isn't ideal. Possibly to restrictive
            //checking for only types with an AutoRoute isn't satisfactory either
            //as what happens for content items that provide own route
            //possibly simple option is provide all content Types

            //also include items that are contained
            var indexableTypeDefs = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Listable && ctd.Settings.ToObject<ContentTypeSettings>().Creatable)
                .OrderBy(ctd => ctd.DisplayName).ToList();

            //hmm this is a bit manky, but rather reduce the list of indexable items than provide a list of too many items.
            //other alterative is list all content types. 
            var listPartTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == "ListPart"));
            foreach(var listPartType in listPartTypes){
                var listPartDef = listPartType.Parts.FirstOrDefault(x => x.Name == "ListPart");
                listPartDef.Settings.TryGetValue("ContainedContentTypes", out JToken value);
                JArray allowedTypes = (JArray)listPartDef.Settings["ContainedContentTypes"];
                foreach(var allowedType in allowedTypes.ToObject<List<string>>())
                {
                    if (!indexableTypeDefs.Any(x => x.Name == allowedType))
                    {
                        var defToAdd = _contentDefinitionManager.ListTypeDefinitions()
                            .FirstOrDefault(x => x.Name == allowedType);
                        indexableTypeDefs.Add(defToAdd);
                    }
                }
            }

            var entries = indexableTypeDefs.Select(x => new ContentTypeSitemapEntryViewModel
            {
                ContentTypeName = x.Name,
                IsChecked = treeNode.ContentTypes.Any(selected => selected.ContentTypeName == x.Name),
                ChangeFrequency = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.ChangeFrequency ?? ChangeFrequency.Daily,
                Priority = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Priority ?? 0.5f,
                TakeAll = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.TakeAll  ?? true,
                Skip = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Skip ?? 0,
                Take = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeName == x.Name)?.Take ?? 50000,
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
                model.CanSupportChildNodes = treeNode.CanSupportChildNodes;
                model.CanBeChildNode = treeNode.CanBeChildNode;
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
                    .Select(x => new ContentTypeSitemapEntry {
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
