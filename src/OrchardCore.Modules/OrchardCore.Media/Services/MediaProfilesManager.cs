using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Services
{
    public class MediaProfilesManager
    {
        private readonly IDocumentManager<MediaProfilesDocument> _documentManager;

        public MediaProfilesManager(IDocumentManager<MediaProfilesDocument> documentManager) => _documentManager = documentManager;

        /// <summary>
        /// Loads the media profiles document from the store for updating and that should not be cached.
        /// </summary>
        public Task<MediaProfilesDocument> LoadMediaProfilesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the media profiles document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<MediaProfilesDocument> GetMediaProfilesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task RemoveMediaProfileAsync(string name)
        {
            var document = await LoadMediaProfilesDocumentAsync();
            document.MediaProfiles.Remove(name);
            await _documentManager.UpdateAsync(document);
        }

        public async Task UpdateMediaProfileAsync(string name, MediaProfile mediaProfile)
        {
            var document = await LoadMediaProfilesDocumentAsync();
            document.MediaProfiles[name.ToLower()] = mediaProfile;
            await _documentManager.UpdateAsync(document);
        }
    }
}
