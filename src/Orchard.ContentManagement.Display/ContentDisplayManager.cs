using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Zones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display
{
    public class DefaultContentDisplayManager : IContentDisplayManager
    {
        private readonly IEnumerable<IDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;

        public DefaultContentDisplayManager(
            IEnumerable<IDisplayHandler> handlers,
            IShapeTableManager shapeTableManager,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<DefaultContentDisplayManager> logger,
            ILayoutAccessor layoutAccessor
            )
        {
            _handlers = handlers;
            _shapeTableManager = shapeTableManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<dynamic> BuildDisplayAsync(IContent content, string displayType, string groupId)
        {
            if(content == null || content.ContentItem == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(content.ContentItem.ContentType);

            JToken stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
            {
                stereotype = "Content";
            }

            var actualShapeType = stereotype.Value<string>();
            var actualDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, groupId, _shapeFactory);
            context.Layout = _layoutAccessor.GetLayout();

            await BindPlacementAsync(context, actualDisplayType, stereotype.Value<string>());

            await _handlers.InvokeAsync(handler => handler.BuildDisplayAsync(context), Logger);

            return context.Shape;
        }

        public async Task<dynamic> BuildEditorAsync(IContent content, string groupId)
        {
            if (content == null || content.ContentItem == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(content.ContentItem.ContentType);

            JToken stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
            {
                stereotype = "Content";
            }

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new UpdateEditorContext(itemShape, content, groupId, _shapeFactory);
            await BindPlacementAsync(context, null, stereotype.Value<string>());

            await _handlers.InvokeAsync(handler => handler.BuildEditorAsync(context), Logger);
            
            return context.Shape;
        }

        public async Task<dynamic> UpdateEditorAsync(IContent content, IUpdateModel updater, string groupInfoId)
        {
            if (content == null || content.ContentItem == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(content.ContentItem.ContentType);
            JToken stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
            {
                stereotype = "Content";
            }

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateContentShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var theme = await _themeManager.GetThemeAsync();
            var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);

            // adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new UpdateEditorContext(itemShape, content, groupInfoId, _shapeFactory);
            await BindPlacementAsync(context, null, stereotype.Value<string>());

            await _handlers.InvokeAsync(handler => handler.UpdateEditorAsync(context, updater), Logger);

            return context.Shape;
        }
        
        private async Task BindPlacementAsync(BuildShapeContext context, string displayType, string stereotype)
        {
            var theme = await _themeManager.GetThemeAsync();
            var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);

            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor))
                {
                    var placementContext = new ShapePlacementContext
                    {
                        Shape = context.Shape
                    };

                    // define which location should be used if none placement is hit
                    descriptor.DefaultPlacement = defaultLocation;

                    var placement = descriptor.Placement(placementContext);
                    if (placement != null)
                    {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return new PlacementInfo
                {
                    Location = defaultLocation,
                    Source = String.Empty
                };
            };
        }

        private dynamic CreateContentShape(string actualShapeType)
        {
            return _shapeFactory.Create(actualShapeType, Arguments.Empty, () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty)));
        }
    }
}
