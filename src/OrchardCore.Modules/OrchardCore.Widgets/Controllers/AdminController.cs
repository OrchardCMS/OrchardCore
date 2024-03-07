using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Controllers
{
    [Admin("Widgets/{action}/{id?}", "Widgets.{action}")]
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
        }

        public async Task<IActionResult> BuildEditor(
            [FromServices] IShapeFactory shapeFactory,
            string id,
            string prefix,
            string prefixesName,
            string contentTypesName,
            string contentItemsName,
            string zonesName,
            string zone,
            string targetId,
            string parentContentType,
            string partName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            contentItem.Weld(new WidgetMetadata());

            var cardCollectionType = nameof(WidgetsListPart);

            // Create a Card Shape
            dynamic contentCard = await shapeFactory.New.ContentCard(
                // Updater is the controller for AJAX Requests
                Updater: this,
                // Shape Specific
                CollectionShapeType: cardCollectionType,
                ContentItem: contentItem,
                BuildEditor: true,
                ParentContentType: parentContentType,
                CollectionPartName: partName,
                // WidgetListPart Specific
                ZoneValue: zone,
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
                ContentItemsName: contentItemsName,
                // Zones
                ZonesId: zonesName.Replace('.', '_'),
                ZonesName: zonesName
            );

            var model = new BuildEditorViewModel
            {
                EditorShape = contentCard
            };

            return View("Display", model);
        }
    }
}
