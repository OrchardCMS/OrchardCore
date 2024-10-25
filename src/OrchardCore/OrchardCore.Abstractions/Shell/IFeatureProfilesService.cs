using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell;

public interface IFeatureProfilesService
{
    Task<IDictionary<string, FeatureProfile>> GetFeatureProfilesAsync();
}
