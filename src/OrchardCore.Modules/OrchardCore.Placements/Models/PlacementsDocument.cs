using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Models;

public class PlacementsDocument : Document
{
    public Dictionary<string, PlacementNode[]> Placements { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
