using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Descriptors;

public class ShapeTablePlacementProvider : IShapePlacementProvider
{
    private readonly IShapeTableManager _shapeTableManager;
    private readonly IThemeManager _themeManager;
    private readonly Dictionary<ShapeTable, Task<IPlacementInfoResolver>> _resolvers = new();

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
                return Task.FromResult<IPlacementInfoResolver>(null);
            }

            if (_resolvers.TryGetValue(shapeTable, out var resolver))
            {
                return resolver;
            }

            resolver = Task.FromResult<IPlacementInfoResolver>(new ShapeTablePlacementResolver(shapeTable));
            _resolvers[shapeTable] = resolver;
            return resolver;
        }

        return BuildPlacementInfoResolverAwaitedAsync(shapeTableTask, _resolvers);

        static async Task<IPlacementInfoResolver> BuildPlacementInfoResolverAwaitedAsync(Task<ShapeTable> shapeTableTask, Dictionary<ShapeTable, Task<IPlacementInfoResolver>> resolvers)
        {
            var shapeTable = await shapeTableTask;

            // If there is no active theme, do nothing
            if (shapeTable == null)
            {
                return null;
            }

            var resolver = new ShapeTablePlacementResolver(shapeTable);
            resolvers[shapeTable] = Task.FromResult<IPlacementInfoResolver>(resolver);
            return resolver;
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

                if (placement != null && !string.IsNullOrEmpty(placementContext.Source))
                {
                    return placement.WithSource(placementContext.Source);
                }

                return placement;
            }

            return null;
        }
    }
}
