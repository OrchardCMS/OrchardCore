using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class ShapeTablePlacementProvider : IShapePlacementProvider
    {
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;

        public ShapeTablePlacementProvider(
            IShapeTableManager shapeTableManager,
            IThemeManager themeManager
            )
        {
            _shapeTableManager = shapeTableManager;
            _themeManager = themeManager;
        }

        public async Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context)
        {
            var theme = await _themeManager.GetThemeAsync();

            // If there is no active theme, do nothing
            if (theme == null)
            {
                return null;
            }

            var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);

            return new ShapeTablePlacementResolver(shapeTable);
        }

        private class ShapeTablePlacementResolver : IPlacementInfoResolver
        {
            private readonly ShapeTable _shapeTable;

            internal ShapeTablePlacementResolver(ShapeTable shapeTable)
            {
                _shapeTable = shapeTable;
            }

            public PlacementInfo ResolvePlacement(ShapePlacementContext placementContext)
            {
                if (_shapeTable.Descriptors.TryGetValue(placementContext.ShapeType, out var descriptor))
                {
                    var placement = descriptor.Placement(placementContext);
                    if (placement != null)
                    {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return null;
            }
        }
    }
}
