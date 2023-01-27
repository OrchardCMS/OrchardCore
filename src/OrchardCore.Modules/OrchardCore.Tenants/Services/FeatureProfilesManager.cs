using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tenants.Services
{
    public class FeatureProfilesManager
    {
        private readonly IDocumentManager<FeatureProfilesDocument> _documentManager;

        public FeatureProfilesManager(IDocumentManager<FeatureProfilesDocument> documentManager) => _documentManager = documentManager;

        /// <summary>
        /// Loads the feature profiles document from the store for updating and that should not be cached.
        /// </summary>
        public Task<FeatureProfilesDocument> LoadFeatureProfilesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the feature profiles document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<FeatureProfilesDocument> GetFeatureProfilesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task RemoveFeatureProfileAsync(string id)
        {
            var document = await LoadFeatureProfilesDocumentAsync();
            document.FeatureProfiles.Remove(id);
            await _documentManager.UpdateAsync(document);
        }

        public async Task UpdateFeatureProfileAsync(string id, FeatureProfile profile)
        {
            var document = await LoadFeatureProfilesDocumentAsync();
            document.FeatureProfiles[id] = profile;
            await _documentManager.UpdateAsync(document);
        }
    }
}
