using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services
{
    public class PlacementsManager
    {
        private readonly IPlacementStore _placementStore;

        public PlacementsManager(IPlacementStore placementStore) => _placementStore = placementStore;

        public async Task<IReadOnlyDictionary<string, IEnumerable<PlacementNode>>> ListShapePlacementsAsync()
        {
            var document = await _placementStore.GetPlacementsAsync();

            return document.Placements.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
        }

        public async Task<IEnumerable<PlacementNode>> GetShapePlacementsAsync(string shapeType)
        {
            var document = await _placementStore.GetPlacementsAsync();

            if (document.Placements.TryGetValue(shapeType, out var nodes))
            {
                return nodes;
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateShapePlacementsAsync(string shapeType, IEnumerable<PlacementNode> placementNodes)
        {
            var document = await _placementStore.LoadPlacementsAsync();

            document.Placements[shapeType] = placementNodes.ToArray();

            await _placementStore.SavePlacementsAsync(document);
        }

        public async Task RemoveShapePlacementsAsync(string shapeType)
        {
            var document = await _placementStore.LoadPlacementsAsync();

            if (document.Placements.Remove(shapeType))
            {
                await _placementStore.SavePlacementsAsync(document);
            }
        }
    }
}
