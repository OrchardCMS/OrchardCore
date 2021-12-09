using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Display
{
    /// <summary>
    /// The default implementation of <see cref="IContentItemDisplayManager"/>. It is used to render
    /// <see cref="ContentItem"/> objects by leveraging any <see cref="IContentDisplayHandler"/>
    /// implementation. The resulting shapes are targeting the stereotype of the content item
    /// to display.
    /// </summary>
    public class ContentItemDisplayManager : BaseDisplayManager, IContentItemDisplayManager
    {
        private readonly IEnumerable<IContentHandler> _contentHandlers;
        private readonly IEnumerable<IContentDisplayHandler> _handlers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ILogger _logger;

        public ContentItemDisplayManager(
            IEnumerable<IContentDisplayHandler> handlers,
            IEnumerable<IContentHandler> contentHandlers,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IEnumerable<IShapePlacementProvider> placementProviders,
            ILogger<ContentItemDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeFactory, placementProviders)
        {
            _handlers = handlers;
            _contentHandlers = contentHandlers;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _layoutAccessor = layoutAccessor;
            _logger = logger;
        }

        public async Task<IShape> BuildDisplayAsync(ContentItem contentItem, IUpdateModel updater, string displayType, string groupId)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            var stereotype = contentTypeDefinition.GetSettings<ContentTypeSettings>().Stereotype;
            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;
            var actualShapeType = stereotype ?? "Content";

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            var itemShape = await CreateContentShapeAsync(actualShapeType);
            itemShape.Properties["ContentItem"] = contentItem;
            itemShape.Properties["Stereotype"] = stereotype;

            var metadata = itemShape.Metadata;
            metadata.DisplayType = actualDisplayType;

            // [Stereotype]_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
            metadata.Alternates.Add($"{actualShapeType}__{contentItem.ContentType}");

            var context = new BuildDisplayContext(
                itemShape,
                actualDisplayType,
                groupId,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync((handler, contentItem, context) => handler.BuildDisplayAsync(contentItem, context), contentItem, context, _logger);

            return context.Shape;
        }

        public async Task<IShape> BuildEditorAsync(ContentItem contentItem, IUpdateModel updater, bool isNew, string groupId, string htmlFieldPrefix)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            var stereotype = contentTypeDefinition.GetSettings<ContentTypeSettings>().Stereotype;

            var actualShapeType = (stereotype ?? "Content") + "_Edit";

            var itemShape = await CreateContentShapeAsync(actualShapeType);
            itemShape.Properties["ContentItem"] = contentItem;
            itemShape.Properties["Stereotype"] = stereotype;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            itemShape.Metadata.Alternates.Add(actualShapeType + "__" + contentItem.ContentType);

            var context = new BuildEditorContext(
                itemShape,
                groupId,
                isNew,
                htmlFieldPrefix,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync((handler, contentItem, context) => handler.BuildEditorAsync(contentItem, context), contentItem, context, _logger);

            return context.Shape;
        }

        public async Task<IShape> UpdateEditorAsync(ContentItem contentItem, IUpdateModel updater, bool isNew, string groupId, string htmlFieldPrefix)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.LoadTypeDefinition(contentItem.ContentType);
            var stereotype = contentTypeDefinition.GetSettings<ContentTypeSettings>().Stereotype;
            var actualShapeType = (stereotype ?? "Content") + "_Edit";

            var itemShape = await CreateContentShapeAsync(actualShapeType);
            itemShape.Properties["ContentItem"] = contentItem;
            itemShape.Properties["Stereotype"] = stereotype;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            itemShape.Metadata.Alternates.Add(actualShapeType + "__" + contentItem.ContentType);

            var context = new UpdateEditorContext(
                itemShape,
                groupId,
                isNew,
                htmlFieldPrefix,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            var updateContentContext = new UpdateContentContext(contentItem);

            await _contentHandlers.InvokeAsync((handler, updateContentContext) => handler.UpdatingAsync(updateContentContext), updateContentContext, _logger);
            await _handlers.InvokeAsync((handler, contentItem, context) => handler.UpdateEditorAsync(contentItem, context), contentItem, context, _logger);
            await _contentHandlers.Reverse().InvokeAsync((handler, updateContentContext) => handler.UpdatedAsync(updateContentContext), updateContentContext, _logger);

            return context.Shape;
        }
    }
}
