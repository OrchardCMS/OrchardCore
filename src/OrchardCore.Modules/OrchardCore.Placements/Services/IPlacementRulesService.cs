using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services
{
    public interface IPlacementRulesService
    {
        IChangeToken ChangeToken { get; }

        IDictionary<string, PlacementNode[]> ListShapePlacements();

        PlacementNode[] GetShapePlacements(string shapeType);

        Task UpdateShapePlacementsAsync(string shapeType, PlacementNode[] placementNodes);

        Task RemoveShapePlacementsAsync(string shapeType);
    }
}
