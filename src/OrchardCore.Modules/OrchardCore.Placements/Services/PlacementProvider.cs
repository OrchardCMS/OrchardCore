using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Placements.Services
{
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
            private readonly IReadOnlyDictionary<string, IEnumerable<PlacementNode>> _placements;
            private readonly IEnumerable<IPlacementNodeFilterProvider> _placementNodeFilterProviders;

            public PlacementInfoResolver(
                IReadOnlyDictionary<string, IEnumerable<PlacementNode>> placements,
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

                        if (filters.Any())
                        {
                            predicate = filters.Aggregate(predicate, BuildPredicate);
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

                        if (!String.IsNullOrEmpty(placementRule.Location))
                        {
                            placement.Location = placementRule.Location;
                        }

                        if (!String.IsNullOrEmpty(placementRule.ShapeType))
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
                }

                return placement;
            }

            private static bool CheckFilter(ShapePlacementContext ctx, PlacementNode filter) => ShapePlacementParsingStrategy.CheckFilter(ctx, filter);

            private Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
                  KeyValuePair<string, JToken> term)
            {
                return ShapePlacementParsingStrategy.BuildPredicate(predicate, term, _placementNodeFilterProviders);
            }
        }
    }
}
