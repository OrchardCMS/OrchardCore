using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.AdminNodes;

public sealed class ListsAdminNodeDriver : DisplayDriver<MenuItem, ListsAdminNode>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ListsAdminNodeDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override Task<IDisplayResult> DisplayAsync(ListsAdminNode treeNode, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ListsAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
            View("ListsAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(ListsAdminNode treeNode, BuildEditorContext context)
    {
        return Initialize<ListsAdminNodeViewModel>("ListsAdminNode_Fields_TreeEdit", async model =>
        {
            model.ContentType = treeNode.ContentType;
            model.ContentTypes = await GetContentTypesSelectListAsync();
            model.IconForContentItems = treeNode.IconForContentItems;
            model.AddContentTypeAsParent = treeNode.AddContentTypeAsParent;
            model.IconForParentLink = treeNode.IconForParentLink;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ListsAdminNode treeNode, UpdateEditorContext context)
    {
        var model = new ListsAdminNodeViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.ContentType, x => x.IconForContentItems,
            m => m.AddContentTypeAsParent,
            m => m.IconForParentLink);

        treeNode.ContentType = model.ContentType;
        treeNode.IconForContentItems = model.IconForContentItems;
        treeNode.AddContentTypeAsParent = model.AddContentTypeAsParent;
        treeNode.IconForParentLink = model.IconForParentLink;

        return Edit(treeNode, context);
    }

    private async Task<List<SelectListItem>> GetContentTypesSelectListAsync()
    {
        return (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(ctd => ctd.Parts.Any(p => p.PartDefinition.Name.Equals(nameof(ListPart), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(ctd => ctd.DisplayName)
            .Select(ctd => new SelectListItem { Value = ctd.Name, Text = ctd.DisplayName })
            .ToList();
    }
}
