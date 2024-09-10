using System.Text.Json.Serialization;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

public class PlacementFile : Dictionary<string, PlacementNode[]>
{
}

public class PlacementNode
{
    [JsonPropertyName("place")]
    public string Location { get; set; }

    [JsonPropertyName("displayType")]
    public string DisplayType { get; set; }

    [JsonPropertyName("differentiator")]
    public string Differentiator { get; set; }

    [JsonPropertyName("alternates")]
    public string[] Alternates { get; set; }

    [JsonPropertyName("wrappers")]
    public string[] Wrappers { get; set; }

    [JsonPropertyName("shape")]
    public string ShapeType { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
}
