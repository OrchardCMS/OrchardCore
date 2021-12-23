using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string contentItemsName, string zonesName, string zone, string targetId, string parentContentType, string partName)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            contentItem.Weld(new WidgetMetadata());

            string cardCollectionType = nameof(WidgetsListPart);

            //Create a Card Shape
            dynamic contentCard = await _shapeFactory.New.ContentCard(
                //Updater is the controller for AJAX Requests
                Updater: _updateModelAccessor.ModelUpdater,
                //Shape Specific
                CollectionShapeType: cardCollectionType,
                ContentItem: contentItem,
                BuildEditor: true,
                ParentContentType: parentContentType,
                CollectionPartName: partName,
                //WidgetListPart Specific
                ZoneValue: zone,
                //Card Specific Properties
                TargetId: targetId,
                Inline: true,
                CanMove: true,
                CanDelete: true,
                //Input hidden
                //Prefixes
                PrefixValue: prefix,
                PrefixesId: prefixesName.Replace('.', '_'),
                PrefixesName: prefixesName,
                //ContentTypes
                ContentTypesId: contentTypesName.Replace('.', '_'),
                ContentTypesName: contentTypesName,
                //ContentItems
                ContentItemsId: contentItemsName.Replace('.', '_'),
                ContentItemsName: contentItemsName,
                //Zones
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
