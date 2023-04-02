using System;
using System.Collections.Generic;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Placements.Services
{
    public partial class PlacementProvider
    {
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
                        if (!PlacementHelper.MatchesAllFilters(placementContext, placementRule, _placementNodeFilterProviders))
                        {
                            // Ignore rule.
                            continue;
                        }

                        placement ??= new PlacementInfo
                        {
                            Source = "OrchardCore.Placements"
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
        }
    }
}
