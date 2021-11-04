using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public class NullFeatureProfilesService : IFeatureProfilesService
    {
        private static readonly IDictionary<string, FeatureProfile> _featureProfiles = new Dictionary<string, FeatureProfile>();

        public Task<IDictionary<string, FeatureProfile>> GetFeatureProfilesAsync()
            => Task.FromResult(_featureProfiles);
    }
}
