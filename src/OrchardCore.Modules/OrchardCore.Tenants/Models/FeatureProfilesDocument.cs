using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Tenants.Models;

public class FeatureProfilesDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<FeatureProfile>))]
    public Dictionary<string, FeatureProfile> FeatureProfiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
