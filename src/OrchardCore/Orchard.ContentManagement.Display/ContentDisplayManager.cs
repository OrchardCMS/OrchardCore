using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Theming;

namespace Orchard.ContentManagement.Display
{
    /// <summary>
    /// The default implementation of <see cref="IContentItemDisplayManager"/>. It is used to render
    /// <see cref="ContentItem"/> objects by leveraging any <see cref="IContentDisplayHandler"/>
    /// implementation. The resulting shapes are targetting the stereotype of the content item
    /// to display.
    /// </summary>
    public class ContentItemDisplayManager : BaseDisplayManager, IContentItemDisplayManager
    {
        private readonly IEnumerable<IContentHandler> _contentHandlers;
        private readonly IEnumerable<IContentDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;

        public ContentItemDisplayManager(
            IEnumerable<IContentDisplayHandler> handlers,
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
            _contentHandlers = contentHandlers;
            _shapeTableManager = shapeTableManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<dynamic> BuildDisplayAsync(ContentItem contentItem, IUpdateModel updater, string displayType, string groupId)
        {
            if(contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            var stereotype = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Stereotype;
            var actualDisplayType = string.IsNullOrEmpty(displayType) ? "Detail" : displayType;
            var actualShapeType = stereotype ?? "Content";

            // _[DisplayType] is only added for the ones different than Detail
            if (actualDisplayType != "Detail")
            {
                actualShapeType = actualShapeType + "_" + actualDisplayType;
            }

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = contentItem;

            ShapeMetadata metadata = itemShape.Metadata;
            metadata.DisplayType = actualDisplayType;

            // [Stereotype]_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
            metadata.Alternates.Add($"{actualShapeType}__{contentItem.ContentType}");

            var context = new BuildDisplayContext(
                itemShape,
                actualDisplayType,
                groupId,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync(handler => handler.BuildDisplayAsync(contentItem, context), Logger);

            return context.Shape;
        }

        public async Task<dynamic> BuildEditorAsync(ContentItem contentItem, IUpdateModel updater, string groupId, string htmlFieldPrefix)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            var stereotype = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Stereotype;

            var actualShapeType = (stereotype ?? "Content") + "_Edit";

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = contentItem;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + contentItem.ContentType);

            var context = new BuildEditorContext(
                itemShape,
                groupId,
                htmlFieldPrefix,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync(handler => handler.BuildEditorAsync(contentItem, context), Logger);

            return context.Shape;
        }

        public async Task<dynamic> UpdateEditorAsync(ContentItem contentItem, IUpdateModel updater, string groupId, string htmlFieldPrefix)
        {
            if (contentItem == null)
            {
                throw new ArgumentNullException(nameof(contentItem));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var stereotype = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Stereotype;
            var actualShapeType = (stereotype ?? "Content") + "_Edit";

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = contentItem;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + contentItem.ContentType);

            var context = new UpdateEditorContext(
                itemShape,
                groupId,
                htmlFieldPrefix,
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            var updateContentContext = new UpdateContentContext(contentItem, updater);

            _contentHandlers.Invoke(handler => handler.Updating(updateContentContext), Logger);

            await _handlers.InvokeAsync(handler => handler.UpdateEditorAsync(contentItem, context), Logger);

            _contentHandlers.Reverse().Invoke(handler => handler.Updated(updateContentContext), Logger);

            return context.Shape;
        }
    }
}
