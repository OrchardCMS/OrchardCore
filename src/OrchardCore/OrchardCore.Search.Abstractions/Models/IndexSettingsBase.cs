using System.Text.Json.Serialization;
using OrchardCore.Entities;

namespace OrchardCore.Search.Models;

public abstract class IndexSettingsBase : Entity
{
    [JsonIgnore]
    public string IndexName { get; set; }
}
