using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Documents;
using OrchardCore.Placements.Models;

namespace OrchardCore.Placements.Services
{
    public class PlacementsManager : IPlacementsManager
    {
        private readonly IDocumentManager<PlacementsDocument> _documentManager;

        public PlacementsManager(IDocumentManager<PlacementsDocument> documentManager) => _documentManager = documentManager;

        public async Task<IReadOnlyDictionary<string, IEnumerable<PlacementNode>>> ListShapePlacementsAsync()
        {
            var document = await _documentManager.GetOrCreateImmutableAsync();

            return document.Placements.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
        }

        public async Task<IEnumerable<PlacementNode>> GetShapePlacementsAsync(string shapeType)
        {
            var document = await _documentManager.GetOrCreateImmutableAsync();

            if (document.Placements.ContainsKey(shapeType))
            {
                return document.Placements[shapeType];
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateShapePlacementsAsync(string shapeType, IEnumerable<PlacementNode> placementNodes)
        {
            var document = await _documentManager.GetOrCreateMutableAsync();

            document.Placements[shapeType] = placementNodes.ToArray();

            await _documentManager.UpdateAsync(document);
        }

        public async Task RemoveShapePlacementsAsync(string shapeType)
        {
            var document = await _documentManager.GetOrCreateMutableAsync();

            if (document.Placements.ContainsKey(shapeType))
            {
                document.Placements.Remove(shapeType);

                await _documentManager.UpdateAsync(document);
            }
        }
    }
}
