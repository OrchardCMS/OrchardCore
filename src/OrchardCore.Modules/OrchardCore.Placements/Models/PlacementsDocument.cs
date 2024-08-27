using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Placements.Models;

public class PlacementsDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<PlacementNode[]>))]
    public Dictionary<string, PlacementNode[]> Placements { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
