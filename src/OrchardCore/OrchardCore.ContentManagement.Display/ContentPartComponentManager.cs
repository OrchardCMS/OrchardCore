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
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Display
{
    /// <summary>
    /// The default implementation of <see cref="IContentItemDisplayManager"/>. It is used to render
    /// <see cref="ContentItem"/> objects by leveraging any <see cref="IContentDisplayHandler"/>
    /// implementation. The resulting shapes are targeting the stereotype of the content item
    /// to display.
    /// </summary>
    public class ContentPartComponentManager : BaseDisplayManager, IContentPartComponentManager
    {
        private readonly IEnumerable<IContentPartDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ILogger _logger;

        public ContentPartComponentManager(
            IEnumerable<IContentPartDisplayHandler> handlers,
            IEnumerable<IContentHandler> contentHandlers,
            IShapeTableManager shapeTableManager,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<ContentItemDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            ) : base(shapeTableManager, shapeFactory, themeManager)
        {
            _handlers = handlers;
            _shapeTableManager = shapeTableManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;
            _logger = logger;
        }

        // This is a shit approach, but the concept is not wrong. i.e. build display on request, instead of upfront.
        public async Task<IShape> BuildDisplayAsync(ContentPart contentPart, IUpdateModel updater, string displayType, string groupId)
        {
            if (contentPart == null)
            {
                throw new ArgumentNullException(nameof(contentPart));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentPart.ContentItem.ContentType);

            // So I guess this coould be useful as a model, but only for consistency.
            // All this is going to do really, is iterate all the zones that the drivers might render stuff into.
            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;
            var actualShapeType = "ContentPartComponent";

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            dynamic itemShape = await CreateContentShapeAsync(actualShapeType);
            itemShape.ContentItem = contentPart;

            ShapeMetadata metadata = itemShape.Metadata;
            metadata.DisplayType = actualDisplayType;

            // [Stereotype]_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
            metadata.Alternates.Add($"{actualShapeType}__{contentPart.ContentItem.ContentType}");

            var context = new BuildDisplayContext(
                itemShape,
                actualDisplayType,
                groupId,
                _shapeFactory,
                await _layoutAccessor.GetLayoutAsync(),
                new ModelStateWrapperUpdater(updater)
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync((handler, contentItem, context) => handler.BuildDisplayAsync(contentPart, context), contentPart, context, _logger);

            return context.Shape;
        }
    }
}
