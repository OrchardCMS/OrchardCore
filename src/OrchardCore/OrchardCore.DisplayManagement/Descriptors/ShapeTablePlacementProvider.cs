using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Descriptors;

public class ShapeTablePlacementProvider : IShapePlacementProvider
{
    private readonly IShapeTableManager _shapeTableManager;
    private readonly IThemeManager _themeManager;
    private readonly ConcurrentDictionary<ShapeTable, IPlacementInfoResolver> _resolvers = [];

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
            return Task.FromResult(GetResolver(shapeTableTask.Result));
        }

        return BuildPlacementInfoResolverAwaitedAsync(shapeTableTask);

        async Task<IPlacementInfoResolver> BuildPlacementInfoResolverAwaitedAsync(Task<ShapeTable> shapeTableTask)
        {
            return GetResolver(await shapeTableTask);
        }
    }

    private IPlacementInfoResolver GetResolver(ShapeTable shapeTable)
    {
        if (shapeTable == null)
        {
            return null;
        }

        return _resolvers.GetOrAdd(shapeTable, static shapeTable => new ShapeTablePlacementResolver(shapeTable));
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
