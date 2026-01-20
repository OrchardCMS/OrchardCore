using OrchardCore.Data.Documents;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.Models;

public class FeatureProfilesDocument : Document
{
    public Dictionary<string, FeatureProfile> FeatureProfiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
