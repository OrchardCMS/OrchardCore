using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Shapes;
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
    private readonly IShapeFactory _shapeFactory;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public AdminController(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IShapeFactory shapeFactory,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _shapeFactory = shapeFactory;
        _updateModelAccessor = updateModelAccessor;
    }

    public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string contentItemsName, string targetId, bool flowMetadata, string parentContentType, string partName, string cardCollectionType = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var contentItem = await _contentManager.NewAsync(id);

        // Does this editor need the flow metadata editor?
        var colSize = 12;
        IEnumerable<ContentTypeDefinition> containedContentTypes = null;

        if (flowMetadata)
        {
            var settings = (await _contentDefinitionManager.GetTypeDefinitionAsync(parentContentType))?.Parts.SingleOrDefault(x => x.Name == partName)?.GetSettings<FlowPartSettings>();
            var metadata = new FlowMetadata();
            contentItem.Weld(metadata);
            colSize = (int)Math.Round(metadata.Size / 100.0 * 12);
            containedContentTypes = await GetContainedContentTypesAsync(settings);
            metadata.Alignment = GetDefaultAlignment(settings);

            // Use provided cardCollectionType or default to FlowPart
            cardCollectionType ??= nameof(FlowPart);
        }
        else
        {
            cardCollectionType ??= nameof(BagPart);
        }

        // Create a Card Shape
        var contentCard = await _shapeFactory.CreateAsync<ContentCardShape>("ContentCard", shape =>
        {
            // Updater is the controller for AJAX Requests
            shape.Updater = _updateModelAccessor.ModelUpdater;
            // Shape Specific
            shape.CollectionShapeType = cardCollectionType;
            shape.ContentItem = contentItem;
            shape.BuildEditor = true;
            shape.ParentContentType = parentContentType;
            shape.CollectionPartName = partName;
            shape.ContainedContentTypes = containedContentTypes;
            // Card Specific Properties
            shape.TargetId = targetId;
            shape.Inline = true;
            shape.CanMove = true;
            shape.CanDelete = true;
            // Input hidden
            // Prefixes
            shape.PrefixValue = prefix;
            shape.PrefixesName = prefixesName;
            // ContentTypes
            shape.ContentTypesName = contentTypesName;
            // ContentItems
            shape.ContentItemsName = contentItemsName;
            // Only Add ColumnSize Property if Part has FlowMetadata
            if (flowMetadata)
            {
                shape.ColumnSize = colSize;
            }
        });

        var model = new BuildEditorViewModel
        {
            EditorShape = contentCard,
        };
        return View("Display", model);
    }

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(FlowPartSettings settings)
    {
        if (settings?.ContainedContentTypes == null || settings.ContainedContentTypes.Length == 0)
        {
            return await _contentDefinitionManager.ListWidgetTypeDefinitionsAsync();
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
    private static FlowAlignment GetDefaultAlignment(FlowPartSettings settings)
    {
        
        if (settings?.DefaultAlignment == null)
        {
            return FlowAlignment.Justify;
        }

        return settings.DefaultAlignment.Value;
    }
}
