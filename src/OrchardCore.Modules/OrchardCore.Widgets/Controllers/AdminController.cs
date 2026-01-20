using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Shapes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Controllers;

[Admin("Widgets/{action}/{id?}", "Widgets.{action}")]
public sealed class AdminController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public AdminController(
        IContentManager contentManager,
        IShapeFactory shapeFactory,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = contentManager;
        _shapeFactory = shapeFactory;
        _updateModelAccessor = updateModelAccessor;
    }

    public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string contentItemsName, string zonesName, string zone, string targetId, string parentContentType, string partName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var contentItem = await _contentManager.NewAsync(id);

        contentItem.Weld(new WidgetMetadata());

        var cardCollectionType = nameof(WidgetsListPart);

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
            // WidgetListPart Specific
            shape.ZoneValue = zone;
            // Card Specific Properties
            shape.TargetId = targetId;
            shape.Inline = true;
            shape.CanMove = true;
            shape.CanDelete = true;
            // Input hidden
            // Prefixes
            shape.PrefixValue = prefix;
            shape.PrefixesId = prefixesName.Replace('.', '_');
            shape.PrefixesName = prefixesName;
            // ContentTypes
            shape.ContentTypesId = contentTypesName.Replace('.', '_');
            shape.ContentTypesName = contentTypesName;
            // ContentItems
            shape.ContentItemsId = contentItemsName.Replace('.', '_');
            shape.ContentItemsName = contentItemsName;
            // Zones
            shape.ZonesId = zonesName.Replace('.', '_');
            shape.ZonesName = zonesName;
        });

        var model = new BuildEditorViewModel
        {
            EditorShape = contentCard,
        };

        return View("Display", model);
    }
}
