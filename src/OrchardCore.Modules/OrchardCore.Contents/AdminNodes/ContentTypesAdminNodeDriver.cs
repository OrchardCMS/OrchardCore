using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNodeDriver : DisplayDriver<MenuItem, ContentTypesAdminNode>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypesAdminNodeDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }
        public override IDisplayResult Display(ContentTypesAdminNode treeNode)
        {
            return Combine(
                View("ContentTypesAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("ContentTypesAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(ContentTypesAdminNode treeNode)
        {
            var listable = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.GetSettings<ContentTypeSettings>().Listable)
                .OrderBy(ctd => ctd.DisplayName).ToList();

            var entries = listable.Select(x => new ContentTypeEntryViewModel
            {
                ContentTypeId = x.Name,
                IsChecked = treeNode.ContentTypes.Any(selected => String.Equals(selected.ContentTypeId, x.Name, StringComparison.OrdinalIgnoreCase)),
                IconClass = treeNode.ContentTypes.Where(selected => selected.ContentTypeId == x.Name).FirstOrDefault()?.IconClass ?? String.Empty
            }).ToArray();

            return Initialize<ContentTypesAdminNodeViewModel>("ContentTypesAdminNode_Fields_TreeEdit", model =>
            {
                model.ShowAll = treeNode.ShowAll;
                model.IconClass = treeNode.IconClass;
                model.ContentTypes = entries;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypesAdminNode treeNode, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            treeNode.ContentTypes = Array.Empty<ContentTypeEntry>();

            var model = new ContentTypesAdminNodeViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.ShowAll, x => x.IconClass, x => x.ContentTypes))
            {
                treeNode.ShowAll = model.ShowAll;
                treeNode.IconClass = model.IconClass;
                treeNode.ContentTypes = model.ContentTypes
                    .Where(x => x.IsChecked == true)
                    .Select(x => new ContentTypeEntry { ContentTypeId = x.ContentTypeId, IconClass = x.IconClass })
                    .ToArray();
            };

            return Edit(treeNode);
        }
    }
}
