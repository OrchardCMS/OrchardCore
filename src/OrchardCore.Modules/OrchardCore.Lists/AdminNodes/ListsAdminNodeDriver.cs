using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.AdminNodes
{
    public class ListsAdminNodeDriver : DisplayDriver<MenuItem, ListsAdminNode>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ListsAdminNodeDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ListsAdminNode treeNode)
        {
            return Combine(
                View("ListsAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
                View("ListsAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
            );
        }

        public override IDisplayResult Edit(ListsAdminNode treeNode)
        {
            return Initialize<ListsAdminNodeViewModel>("ListsAdminNode_Fields_TreeEdit", model =>
            {
                model.ContentType = treeNode.ContentType;
                model.ContentTypes = GetContenTypesSelectList();
                model.IconForContentItems = treeNode.IconForContentItems;
                model.AddContentTypeAsParent = treeNode.AddContentTypeAsParent;
                model.IconForParentLink = treeNode.IconForParentLink;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ListsAdminNode treeNode, IUpdateModel updater)
        {
            var model = new ListsAdminNodeViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix,
                x => x.ContentType, x => x.IconForContentItems,
                x => x.AddContentTypeAsParent, x => x.IconForParentLink))
            {
                treeNode.ContentType = model.ContentType;
                treeNode.IconForContentItems = model.IconForContentItems;
                treeNode.AddContentTypeAsParent = model.AddContentTypeAsParent;
                treeNode.IconForParentLink = model.IconForParentLink;
            };

            return Edit(treeNode);
        }

        private List<SelectListItem> GetContenTypesSelectList()
        {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(p => p.PartDefinition.Name.Equals(typeof(ListPart).Name, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(ctd => ctd.DisplayName)
                .Select(ctd => new SelectListItem { Value = ctd.Name, Text = ctd.DisplayName })
                .ToList();
        }
    }
}
