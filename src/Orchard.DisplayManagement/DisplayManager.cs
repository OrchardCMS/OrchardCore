using System.Threading.Tasks;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Zones;

namespace Orchard.DisplayManagement
{
    public abstract class BaseDisplayManager
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

        protected async Task BindPlacementAsync(IBuildShapeContext context)
        {
            var theme = await _themeManager.GetThemeAsync();
            var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);

            context.FindPlacement = (shape, differentiator, displayType) => FindPlacementImpl(shapeTable, shape, differentiator, displayType);
        }

        private static PlacementInfo FindPlacementImpl(ShapeTable shapeTable, IShape shape, string differentiator, string displayType)
        {
            ShapeDescriptor descriptor;
            var shapeType = shape.Metadata.Type;

            if (shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
            {
                var placementContext = new ShapePlacementContext
                {
                    Shape = shape,
                    DisplayType = displayType,
                    Differentiator = differentiator
                };

                var placement = descriptor.Placement(placementContext);
                if (placement != null)
                {
                    placement.Source = placementContext.Source;
                    return placement;
                }
            }

            return null;

        }

        protected dynamic CreateContentShape(string actualShapeType)
        {
            return _shapeFactory.Create(actualShapeType, Arguments.Empty, () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty)));
        }
    }
}
