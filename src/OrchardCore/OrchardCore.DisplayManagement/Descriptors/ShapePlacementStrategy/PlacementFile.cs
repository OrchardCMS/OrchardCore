using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public class PlacementFile : Dictionary<string, PlacementNode[]>
    {
    }

    public class PlacementNode
    {
        [JsonProperty(PropertyName = "place")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "displayType")]
        public string DisplayType { get; set; }

        [JsonProperty(PropertyName = "differentiator")]
        public string Differentiator { get; set; }

        [JsonProperty(PropertyName = "alternates")]
        public string[] Alternates { get; set; }

        [JsonProperty(PropertyName = "wrappers")]
        public string[] Wrappers { get; set; }

        [JsonProperty(PropertyName = "shape")]
        public string ShapeType { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Filters { get; set; } = new Dictionary<string, JToken>();
    }
}
