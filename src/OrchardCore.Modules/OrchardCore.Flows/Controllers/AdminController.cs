using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        private const string ModuleArea = "OrchardCore.Flows";

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor,
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _updateModelAccessor = updateModelAccessor;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string targetId, bool flowmetadata, string parentContentType, string partName)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            // Does this editor need the flow metadata editor?
            string cardCollectionType = null;
            int colSize = 12;
            if (flowmetadata)
            {
                var metadata = new FlowMetadata();
                contentItem.Weld(metadata);
                colSize = (int)Math.Round((double)metadata.Size / 100.0 * 12);

                cardCollectionType = nameof(FlowPart);
            }
            else
            {
                cardCollectionType = nameof(BagPart);
            }

            // Get Contained Content Types
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(parentContentType);
            IEnumerable<ContentTypeDefinition> containedContentTypeDefinitions = null;

            foreach (var part in contentTypeDefinition.Parts)
            {
                if (string.Equals(part.Name, partName, StringComparison.OrdinalIgnoreCase))
                {
                    IEnumerable<string> contentTypes = null;

                    if (part.PartDefinition.Name == nameof(FlowPart))
                    {
                        var settings = part.GetSettings<FlowPartSettings>();
                        contentTypes = settings.ContainedContentTypes;
                        containedContentTypeDefinitions = GetContainedContentTypes(contentTypes, true);
                    }

                    else if (part.PartDefinition.Name == nameof(BagPart))
                    {
                        var settings = part.GetSettings<BagPartSettings>();
                        contentTypes = settings.ContainedContentTypes;
                        containedContentTypeDefinitions = GetContainedContentTypes(contentTypes, false);
                    }

                    break;
                }
            }

            //Create a Card Shape
            dynamic contentCard = await _shapeFactory.New.ContentCard(
                // Updater is the controller for AJAX Requests
                Updater: _updateModelAccessor.ModelUpdater,
                // Shape Specific
                CollectionShapeType: cardCollectionType,
                ContentItem: contentItem,
                BuildEditor: true,
                ParentContentType: parentContentType,
                CollectionPartName: partName,
                ContainedContentTypes: containedContentTypeDefinitions,
                // Card Specific Properties
                TargetId: targetId,
                Inline: true,
                CanMove: true,
                CanDelete: true,
                InsertArea: ModuleArea,
                // Input hidden
                // Prefixes
                HtmlFieldPrefix: prefix,
                PrefixesId: prefixesName.Replace('.', '_'),
                PrefixesName: prefixesName,
                // ContentTypes
                ContentTypesId: contentTypesName.Replace('.', '_'),
                ContentTypesName: contentTypesName
            );
            // If Part has FlowMetadata
            if (flowmetadata)
            {
                contentCard.ColumnSize = colSize;
                contentCard.CanInsert = true;
                contentCard.HasFlowMetadata = true;
            }
            else
            {
                contentCard.CanInsert = false;
                contentCard.HasFlowMetadata = false;
            }

            var model = new BuildEditorViewModel
            {
                EditorShape = contentCard
            };
            return View("Display", model);
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(IEnumerable<string> contentTypes, bool onlyWidgets)
        {
            if (contentTypes == null || !contentTypes.Any())
            {
                return _contentDefinitionManager.ListTypeDefinitions().Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget");
            }

            if (onlyWidgets)
            {
                return contentTypes
                    .Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType))
                    .Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget");
            }
            else
            {
                return contentTypes
                    .Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
            }
        }
    }
}
