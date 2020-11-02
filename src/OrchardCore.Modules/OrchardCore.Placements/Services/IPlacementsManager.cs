using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services
{
    public interface IPlacementsManager
    {
        Task<IReadOnlyDictionary<string, IEnumerable<PlacementNode>>> ListShapePlacementsAsync();

        Task<IEnumerable<PlacementNode>> GetShapePlacementsAsync(string shapeType);

        Task UpdateShapePlacementsAsync(string shapeType, IEnumerable<PlacementNode> placementNodes);

        Task RemoveShapePlacementsAsync(string shapeType);
    }
}
