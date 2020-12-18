using System.Threading.Tasks;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.Documents;

namespace OrchardCore.BackgroundTasks.Services
{
    public class BackgroundTaskManager
    {
        private readonly IDocumentManager<BackgroundTaskDocument> _documentManager;

        public BackgroundTaskManager(IDocumentManager<BackgroundTaskDocument> documentManager)
        {
            _documentManager = documentManager;
        }

        /// <summary>
        /// Loads the background task document from the store for updating and that should not be cached.
        /// </summary>
        public Task<BackgroundTaskDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the background task document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<BackgroundTaskDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task RemoveAsync(string name)
        {
            var document = await LoadDocumentAsync();
            document.Settings.Remove(name);
            await _documentManager.UpdateAsync(document);
        }

        public async Task UpdateAsync(string name, BackgroundTaskSettings settings)
        {
            var document = await LoadDocumentAsync();
            document.Settings[name] = settings;
            await _documentManager.UpdateAsync(document);
        }
    }
}
