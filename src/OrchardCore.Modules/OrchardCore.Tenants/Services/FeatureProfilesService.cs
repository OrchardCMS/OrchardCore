using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.Services
{
    public class FeatureProfilesService : IFeatureProfilesService
    {
        private readonly FeatureProfilesManager _featureProfilesManager;

        public FeatureProfilesService(FeatureProfilesManager featureProfilesManager)
        {
            _featureProfilesManager = featureProfilesManager;
        }

        public async Task<IDictionary<string, FeatureProfile>> GetFeatureProfilesAsync()
        {
            var document = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();

            return document.FeatureProfiles;
        }
    }
}
