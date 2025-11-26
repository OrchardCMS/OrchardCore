using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Placements.Services;

public class PlacementProvider : IShapePlacementProvider
{
    private readonly PlacementsManager _placementsManager;
    private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;

    public PlacementProvider(
        PlacementsManager placementsManager,
        IEnumerable<IPlacementNodeFilterProvider> placementNodeFilterProviders)
    {
        _placementsManager = placementsManager;
        _placementNodeFilterProviders = placementNodeFilterProviders;
    }

    public async Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context)
    {
        var placements = await _placementsManager.ListShapePlacementsAsync();
        return new PlacementInfoResolver(placements, _placementNodeFilterProviders);
    }

    public class PlacementInfoResolver : IPlacementInfoResolver
    {
        private readonly IReadOnlyDictionary<string, PlacementNode[]> _placements;
        private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;

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

            if (_placements.ContainsKey(placementContext.ShapeType))
            {
                var shapePlacements = _placements[placementContext.ShapeType];

                foreach (var placementRule in shapePlacements)
                {
                    var filters = placementRule.Filters.ToList();

                    Func<ShapePlacementContext, bool> predicate = ctx => CheckFilter(ctx, placementRule);

                    if (filters.Count > 0)
                    {
                        predicate = filters.Aggregate(predicate, BuildPredicate);
                    }

                    if (!predicate(placementContext))
                    {
                        // Ignore rule
                        continue;
                    }

                    var alternates = placementRule.Alternates?.Length > 0
                        ? (placement?.Alternates).Combine(new AlternatesCollection(placementRule.Alternates))
                        : placement?.Alternates;

                    var wrappers = placementRule.Wrappers?.Length > 0
                        ? (placement?.Wrappers).Combine(new AlternatesCollection(placementRule.Wrappers))
                        : placement?.Wrappers;

                    placement = new PlacementInfo
                    {
                        Source = placement == null ? "OrchardCore.Placements" : $"{placement.Source},OrchardCore.Placements",
                        Location = !string.IsNullOrEmpty(placementRule.Location) ? placementRule.Location : placement?.Location,
                        ShapeType = !string.IsNullOrEmpty(placementRule.ShapeType) ? placementRule.ShapeType : placement?.ShapeType,
                        DefaultPosition = placement?.DefaultPosition,
                        Alternates = alternates,
                        Wrappers = wrappers,
                    };
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
