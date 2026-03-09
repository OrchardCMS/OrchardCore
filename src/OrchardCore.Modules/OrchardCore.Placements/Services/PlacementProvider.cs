using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Handlers;

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

            string combinedSource = null;
            string currentLocation = null;
            string currentShapeType = null;
            string[] combinedAlternates = null;
            string[] combinedWrappers = null;

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

                // Update source
                combinedSource = combinedSource == null ? "OrchardCore.Placements" : $"{combinedSource},OrchardCore.Placements";

                // Update location if rule provides one
                if (!string.IsNullOrEmpty(placementRule.Location))
                {
                    currentLocation = placementRule.Location;
                }

                // Update shape type if rule provides one
                if (!string.IsNullOrEmpty(placementRule.ShapeType))
                {
                    currentShapeType = placementRule.ShapeType;
                }

                // Combine alternates
                if (placementRule.Alternates != null && placementRule.Alternates.Length > 0)
                {
                    if (combinedAlternates == null || combinedAlternates.Length == 0)
                    {
                        combinedAlternates = placementRule.Alternates;
                    }
                    else
                    {
                        var newAlternates = new string[combinedAlternates.Length + placementRule.Alternates.Length];
                        Array.Copy(combinedAlternates, 0, newAlternates, 0, combinedAlternates.Length);
                        Array.Copy(placementRule.Alternates, 0, newAlternates, combinedAlternates.Length, placementRule.Alternates.Length);
                        combinedAlternates = newAlternates;
                    }
                }

                // Combine wrappers
                if (placementRule.Wrappers != null && placementRule.Wrappers.Length > 0)
                {
                    if (combinedWrappers == null || combinedWrappers.Length == 0)
                    {
                        combinedWrappers = placementRule.Wrappers;
                    }
                    else
                    {
                        var newWrappers = new string[combinedWrappers.Length + placementRule.Wrappers.Length];
                        Array.Copy(combinedWrappers, 0, newWrappers, 0, combinedWrappers.Length);
                        Array.Copy(placementRule.Wrappers, 0, newWrappers, combinedWrappers.Length, placementRule.Wrappers.Length);
                        combinedWrappers = newWrappers;
                    }
                }
            }

            // Only create PlacementInfo if we found matching rules
            if (combinedSource != null)
            {
                placement = new PlacementInfo(
                    currentLocation,
                    combinedSource,
                    currentShapeType,
                    null,
                    combinedAlternates,
                    combinedWrappers
                );
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
