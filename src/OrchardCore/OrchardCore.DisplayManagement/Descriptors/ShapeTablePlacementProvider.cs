using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Descriptors;

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

    public Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context)
    {
        var shapeTableTask = _themeManager.TryGetShapeTableAsync(_shapeTableManager);

        if (shapeTableTask.IsCompletedSuccessfully)
        {
            var shapeTable = shapeTableTask.Result;

            // If there is no active theme, do nothing
            if (shapeTable == null)
            {
                return null;
            }

            return Task.FromResult<IPlacementInfoResolver>(new ShapeTablePlacementResolver(shapeTable));
        }

        return BuildPlacementInfoResolverAwaitedAsync(shapeTableTask);

        static async Task<IPlacementInfoResolver> BuildPlacementInfoResolverAwaitedAsync(Task<ShapeTable> shapeTableTask)
        {
            var shapeTable = await shapeTableTask;

            // If there is no active theme, do nothing
            if (shapeTable == null)
            {
                return null;
            }

            return new ShapeTablePlacementResolver(shapeTable);
        }
    }

    private sealed class ShapeTablePlacementResolver : IPlacementInfoResolver
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
