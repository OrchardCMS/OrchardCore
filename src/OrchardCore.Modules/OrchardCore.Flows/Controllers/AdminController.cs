using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Controllers
{
    public class AdminController : Controller
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

        public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string contentItemsName, string targetId, bool flowmetadata, string parentContentType, string partName)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            // Does this editor need the flow metadata editor?
            string cardCollectionType = null;
            int colSize = 12;
            IEnumerable<ContentTypeDefinition> containedContentTypes = null;

            if (flowmetadata)
            {
                var metadata = new FlowMetadata();
                contentItem.Weld(metadata);
                colSize = (int)Math.Round((double)metadata.Size / 100.0 * 12);
                containedContentTypes = GetContainedContentTypes(parentContentType, partName);

                cardCollectionType = nameof(FlowPart);
            }
            else
            {
                cardCollectionType = nameof(BagPart);
            }

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
                ContainedContentTypes: containedContentTypes,
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
                ContentItemsName: contentItemsName
            );
            //Only Add ColumnSize Property if Part has FlowMetadata
            if (flowmetadata)
            {
                contentCard.ColumnSize = colSize;
            }

            var model = new BuildEditorViewModel
            {
                EditorShape = contentCard
            };
            return View("Display", model);
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(string contentType, string partName)
        {
            var settings = _contentDefinitionManager.GetTypeDefinition(contentType)?.Parts.SingleOrDefault(x => x.Name == partName)?.GetSettings<FlowPartSettings>();

            if (settings == null || settings.ContainedContentTypes == null || !settings.ContainedContentTypes.Any())
            {
                return _contentDefinitionManager.ListTypeDefinitions().Where(t => t.GetStereotype() == "Widget");
            }

            return settings.ContainedContentTypes
                .Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType))
                .Where(t => t != null && t.GetStereotype() == "Widget");
        }
    }
}
