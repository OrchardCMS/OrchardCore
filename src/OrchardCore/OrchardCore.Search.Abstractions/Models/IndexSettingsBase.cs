using System.Text.Json.Serialization;

namespace OrchardCore.Search.Models;

public abstract class IndexSettingsBase
{
    [JsonIgnore]
    public string IndexName { get; set; }
}
