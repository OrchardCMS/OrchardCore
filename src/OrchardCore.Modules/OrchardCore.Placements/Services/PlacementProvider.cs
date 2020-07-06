using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Placements.Services
{
    public class PlacementProvider : ITenantShapeTableProvider
    {
        private readonly IPlacementRulesService _placementRulesService;
        private readonly IPlacementNodeProcessor _placementNodeProcessor;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public PlacementProvider(
            IPlacementRulesService placementRulesService,
            IPlacementNodeProcessor placementFileProcessor,
            ITypeFeatureProvider typeFeatureProvider)
        {
            _placementRulesService = placementRulesService;
            _placementNodeProcessor = placementFileProcessor;
            _typeFeatureProvider = typeFeatureProvider;
        }

        public IChangeToken ChangeToken => _placementRulesService.ChangeToken;

        public void Discover(ShapeTableBuilder builder)
        {
            var placementRules = _placementRulesService.ListShapePlacements();
            var featureDescriptor = _typeFeatureProvider.GetFeatureForDependency(typeof(PlacementProvider));

            foreach(var placementRule in placementRules)
            {
                foreach(var placementNode in placementRule.Value)
                {
                    _placementNodeProcessor.ProcessPlacementNode(builder, featureDescriptor, placementRule.Key, placementNode);
                }
            }
        }
    }
}
