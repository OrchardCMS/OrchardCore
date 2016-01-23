using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Zones;
using System;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement
{
    public abstract class BaseDisplayManager<TModel>
    {
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;

        public BaseDisplayManager(
            IShapeTableManager shapeTableManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager
            )
        {
            _shapeTableManager = shapeTableManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
        }

        ILogger Logger { get; set; }
                
        protected async Task BindPlacementAsync(BuildShapeContext<TModel> context, string displayType, string stereotype)
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

        protected dynamic CreateContentShape(string actualShapeType)
        {
            return _shapeFactory.Create(actualShapeType, Arguments.Empty, () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty)));
        }
    }
}
