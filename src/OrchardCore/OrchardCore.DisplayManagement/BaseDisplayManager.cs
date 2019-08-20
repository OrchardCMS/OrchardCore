using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement
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

            // If there is no active theme, do nothing
            if (theme == null)
            {
                return;
            }

            var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);

            context.FindPlacement = (shapeType, differentiator, displayType, displayContext) => FindPlacementImpl(shapeTable, shapeType, differentiator, displayType, context);
        }

        private static PlacementInfo FindPlacementImpl(ShapeTable shapeTable, string shapeType, string differentiator, string displayType, IBuildShapeContext context)
        {
            ShapeDescriptor descriptor;

            if (shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
            {
                var placementContext = new ShapePlacementContext(
                    shapeType,
                    displayType,
                    differentiator,
                    context.Shape
                );

                var placement = descriptor.Placement(placementContext);
                if (placement != null)
                {
                    placement.Source = placementContext.Source;
                    return placement;
                }
            }

            return null;
        }

        protected Task<IShape> CreateContentShapeAsync(string actualShapeType)
        {
            return _shapeFactory.CreateAsync(actualShapeType, () => Task.FromResult<IShape>(new ZoneHolding(() => _shapeFactory.CreateAsync("ContentZone", Arguments.Empty))));
        }
    }
}
