using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Handlers;

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
    }
}
