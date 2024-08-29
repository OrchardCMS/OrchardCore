using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Controllers;

[Admin("Flows/{action}", "Flows.{action}")]
public sealed class AdminController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public AdminController(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IShapeFactory shapeFactory,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _shapeFactory = shapeFactory;
        _updateModelAccessor = updateModelAccessor;
    }

    public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string contentItemsName, string targetId, bool flowMetadata, string parentContentType, string partName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var contentItem = await _contentManager.NewAsync(id);

        // Does this editor need the flow metadata editor?
        string cardCollectionType = null;
        var colSize = 12;
        IEnumerable<ContentTypeDefinition> containedContentTypes = null;

        if (flowMetadata)
        {
            var metadata = new FlowMetadata();
            contentItem.Weld(metadata);
            colSize = (int)Math.Round(metadata.Size / 100.0 * 12);
            containedContentTypes = await GetContainedContentTypesAsync(parentContentType, partName);

            cardCollectionType = nameof(FlowPart);
        }
        else
        {
            cardCollectionType = nameof(BagPart);
        }

        // Create a Card Shape
        var contentCard = await _shapeFactory.New.ContentCard(
            // Updater is the controller for AJAX Requests
            Updater: _updateModelAccessor.ModelUpdater,
            // Shape Specific
            CollectionShapeType: cardCollectionType,
            ContentItem: contentItem,
            BuildEditor: true,
            ParentContentType: parentContentType,
            CollectionPartName: partName,
            ContainedContentTypes: containedContentTypes,
            // Card Specific Properties
            TargetId: targetId,
            Inline: true,
            CanMove: true,
            CanDelete: true,
            // Input hidden
            // Prefixes
            PrefixValue: prefix,
            PrefixesId: prefixesName.Replace('.', '_'),
            PrefixesName: prefixesName,
            // ContentTypes
            ContentTypesId: contentTypesName.Replace('.', '_'),
            ContentTypesName: contentTypesName,
            // ContentItems
            ContentItemsId: contentItemsName.Replace('.', '_'),
            ContentItemsName: contentItemsName
        );
        // Only Add ColumnSize Property if Part has FlowMetadata
        if (flowMetadata)
        {
            contentCard.ColumnSize = colSize;
        }

        var model = new BuildEditorViewModel
        {
            EditorShape = contentCard
        };
        return View("Display", model);
    }

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(string contentType, string partName)
    {
        var settings = (await _contentDefinitionManager.GetTypeDefinitionAsync(contentType))?.Parts.SingleOrDefault(x => x.Name == partName)?.GetSettings<FlowPartSettings>();

        if (settings?.ContainedContentTypes == null || settings.ContainedContentTypes.Length == 0)
        {
            return (await _contentDefinitionManager.ListTypeDefinitionsAsync()).Where(t => t.StereotypeEquals("Widget"));
        }

        var definitions = new List<ContentTypeDefinition>();

        foreach (var ct in settings.ContainedContentTypes)
        {
            var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(ct);

            if (definition == null || !definition.StereotypeEquals("Widget"))
            {
                continue;
            }

            definitions.Add(definition);
        }

        return definitions;
    }
}
