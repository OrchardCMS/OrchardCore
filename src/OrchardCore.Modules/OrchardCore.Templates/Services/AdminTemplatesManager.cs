using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services
{
    public class AdminTemplatesManager
    {
        private readonly IDocumentManager<AdminTemplatesDocument> _documentManager;

        public AdminTemplatesManager(IDocumentManager<AdminTemplatesDocument> documentManager) => _documentManager = documentManager;

        /// <summary>
        /// Loads the templates document from the store for updating and that should not be cached.
        /// </summary>
        public Task<AdminTemplatesDocument> LoadTemplatesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the background task document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<AdminTemplatesDocument> GetTemplatesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task RemoveTemplateAsync(string name)
        {
            var document = await LoadTemplatesDocumentAsync();
            document.Templates.Remove(name);
            await _documentManager.UpdateAsync(document);
        }

        public async Task UpdateTemplateAsync(string name, Template template)
        {
            var document = await LoadTemplatesDocumentAsync();
            document.Templates[name] = template;
            await _documentManager.UpdateAsync(document);
        }
    }
}
