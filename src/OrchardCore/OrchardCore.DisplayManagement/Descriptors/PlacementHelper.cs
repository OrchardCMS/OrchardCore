using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.DisplayManagement.Descriptors;

public static class PlacementHelper
{
    public static bool MatchesAllFilters(ShapePlacementContext context, PlacementNode placementNode, IEnumerable<IPlacementNodeFilterProvider> placementNodeFilterProviders)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (placementNode == null)
        {
            throw new ArgumentNullException(nameof(placementNode));
        }

        if (!String.IsNullOrEmpty(placementNode.DisplayType) && placementNode.DisplayType != context.DisplayType)
        {
            return false;
        }

        if (!String.IsNullOrEmpty(placementNode.Differentiator) && placementNode.Differentiator != context.Differentiator)
        {
            return false;
        }

        if (placementNodeFilterProviders != null && placementNodeFilterProviders.Any())
        {
            foreach (var filter in placementNode.Filters)
            {
                var filterProviders = placementNodeFilterProviders.Where(x => x.Key == filter.Key).ToList();

                foreach (var filterProvider in filterProviders)
                {
                    if (!filterProvider.IsMatch(context, filter.Value))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
