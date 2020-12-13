using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Shortcodes.Models;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeTemplatesManager
    {
        private readonly IDocumentManager<ShortcodeTemplatesDocument> _documentManager;

        public ShortcodeTemplatesManager(IDocumentManager<ShortcodeTemplatesDocument> documentManager) => _documentManager = documentManager;

        /// <summary>
        /// Loads the shortcode templates document from the store for updating and that should not be cached.
        /// </summary>
        public Task<ShortcodeTemplatesDocument> LoadShortcodeTemplatesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the shortcode templates document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<ShortcodeTemplatesDocument> GetShortcodeTemplatesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public async Task RemoveShortcodeTemplateAsync(string name)
        {
            var document = await LoadShortcodeTemplatesDocumentAsync();
            document.ShortcodeTemplates.Remove(name);
            await _documentManager.UpdateAsync(document);
        }

        public async Task UpdateShortcodeTemplateAsync(string name, ShortcodeTemplate template)
        {
            var document = await LoadShortcodeTemplatesDocumentAsync();
            document.ShortcodeTemplates[name.ToLower()] = template;
            await _documentManager.UpdateAsync(document);
        }
    }
}
