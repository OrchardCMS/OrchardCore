using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    /// <summary>
    /// This component processes the content of placement file and applies alteration to ShapeTableBuilder
    /// </summary>
    public class PlacementNodeProcessor : IPlacementNodeProcessor
    {
        private readonly IEnumerable<IPlacementNodeFilterProvider> _placementParseMatchProviders;

        public PlacementNodeProcessor(
            IEnumerable<IPlacementNodeFilterProvider> placementParseMatchProviders)
        {
            _placementParseMatchProviders = placementParseMatchProviders;
        }

        public void ProcessPlacementNode(ShapeTableBuilder builder, IFeatureInfo featureDescriptor, string shapeType, PlacementNode placementNode)
        {
            var matches = placementNode.Filters.ToList();

            Func<ShapePlacementContext, bool> predicate = ctx => CheckFilter(ctx, placementNode);

            if (matches.Any())
            {
                predicate = matches.Aggregate(predicate, BuildPredicate);
            }

            var placement = new PlacementInfo();

            placement.Location = placementNode.Location;
            if (placementNode.Alternates?.Length > 0)
            {
                placement.Alternates = new AlternatesCollection(placementNode.Alternates);
            }

            if (placementNode.Wrappers?.Length > 0)
            {
                placement.Wrappers = new AlternatesCollection(placementNode.Wrappers);
            }

            placement.ShapeType = placementNode.ShapeType;

            builder.Describe(shapeType)
                .From(featureDescriptor)
                .Placement(ctx => predicate(ctx), placement);
        }

        public static bool CheckFilter(ShapePlacementContext ctx, PlacementNode filter)
        {
            if (!String.IsNullOrEmpty(filter.DisplayType) && filter.DisplayType != ctx.DisplayType)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(filter.Differentiator) && filter.Differentiator != ctx.Differentiator)
            {
                return false;
            }

            return true;
        }

        private Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
              KeyValuePair<string, JToken> term)
        {
            return BuildPredicate(predicate, term, _placementParseMatchProviders);
        }

        public static Func<ShapePlacementContext, bool> BuildPredicate(Func<ShapePlacementContext, bool> predicate,
                KeyValuePair<string, JToken> term, IEnumerable<IPlacementNodeFilterProvider> placementMatchProviders)
        {
            if (placementMatchProviders != null)
            {
                var providersForTerm = placementMatchProviders.Where(x => x.Key.Equals(term.Key));
                if (providersForTerm.Any())
                {
                    var expression = term.Value;
                    return ctx => providersForTerm.Any(x => x.IsMatch(ctx, expression)) && predicate(ctx);
                }
            }
            return predicate;
        }
    }
}
