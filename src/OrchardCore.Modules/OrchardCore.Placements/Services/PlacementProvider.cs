using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Placements.Services;

public class PlacementProvider : IShapePlacementProvider
{
    private readonly PlacementsManager _placementsManager;
    private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;
    private Task<IPlacementInfoResolver> _resolver;

    public PlacementProvider(
        PlacementsManager placementsManager,
        IEnumerable<IPlacementNodeFilterProvider> placementNodeFilterProviders)
    {
        _placementsManager = placementsManager;
        _placementNodeFilterProviders = placementNodeFilterProviders;
    }

    public Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context)
    {
        if (_resolver != null)
        {
            return _resolver;
        }

        return BuildResolverAsync(this);

        static async Task<IPlacementInfoResolver> BuildResolverAsync(PlacementProvider provider)
        {
            var placements = await provider._placementsManager.ListShapePlacementsAsync();
            var resolver = new PlacementInfoResolver(placements, provider._placementNodeFilterProviders);
            provider._resolver = Task.FromResult<IPlacementInfoResolver>(resolver);
            return resolver;
        }
    }

    private sealed class PlacementInfoResolver : IPlacementInfoResolver
    {
        private readonly IReadOnlyDictionary<string, PlacementNode[]> _placements;
        private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;
        private readonly Dictionary<PlacementNode, Func<ShapePlacementContext, bool>> _predicateCache = new();

        public PlacementInfoResolver(
            IReadOnlyDictionary<string, PlacementNode[]> placements,
            IEnumerable<IPlacementNodeFilterProvider> placementNodeFilterProviders)
        {
            _placements = placements;
            _placementNodeFilterProviders = placementNodeFilterProviders;
        }

        public PlacementInfo ResolvePlacement(ShapePlacementContext placementContext)
        {
            PlacementInfo placement = null;

            if (!_placements.TryGetValue(placementContext.ShapeType, out var shapePlacements))
            {
                return placement;
            }

            foreach (var placementRule in shapePlacements)
            {
                var filters = placementRule.Filters;

                if (!_predicateCache.TryGetValue(placementRule, out var predicate))
                {
                    predicate = ctx => CheckFilter(ctx, placementRule);

                    if (filters.Count > 0)
                    {
                        predicate = filters.Aggregate(predicate, BuildPredicate);
                    }

                    _predicateCache[placementRule] = predicate;
                }

                if (!predicate(placementContext))
                {
                    // Ignore rule
                    continue;
                }

                placement ??= new PlacementInfo
                {
                    Source = "OrchardCore.Placements",
                };

                if (!string.IsNullOrEmpty(placementRule.Location))
                {
                    placement.Location = placementRule.Location;
                }

                if (!string.IsNullOrEmpty(placementRule.ShapeType))
                {
                    placement.ShapeType = placementRule.ShapeType;
                }

                if (placementRule.Alternates?.Length > 0)
                {
                    placement.Alternates = placement.Alternates.Combine(new AlternatesCollection(placementRule.Alternates));
                }

                if (placementRule.Wrappers?.Length > 0)
                {
                    placement.Wrappers = placement.Wrappers.Combine(new AlternatesCollection(placementRule.Wrappers));
                }
            }

            return placement;
        }

        private static bool CheckFilter(ShapePlacementContext ctx, PlacementNode filter) => ShapePlacementParsingStrategy.CheckFilter(ctx, filter);

        private Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
              KeyValuePair<string, object> term)
        {
            return ShapePlacementParsingStrategy.BuildPredicate(predicate, term, _placementNodeFilterProviders);
        }
    }
}
