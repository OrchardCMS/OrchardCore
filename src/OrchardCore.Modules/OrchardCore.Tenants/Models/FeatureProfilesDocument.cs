using OrchardCore.Data.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Tenants.Models;

public class FeatureProfilesDocument : Document
{
    private readonly Dictionary<string, FeatureProfile> _featureProfiles = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, FeatureProfile> FeatureProfiles
    {
        get => _featureProfiles;
        set => _featureProfiles.SetItems(value);
    }
}
