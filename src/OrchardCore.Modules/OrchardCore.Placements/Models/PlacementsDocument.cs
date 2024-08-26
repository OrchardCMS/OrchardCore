using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Modules;

namespace OrchardCore.Placements.Models;

public class PlacementsDocument : Document
{
    private readonly Dictionary<string, PlacementNode[]> _placements = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, PlacementNode[]> Placements
    {
        get => _placements;
        set => _placements.SetItems(value);
    }
}
