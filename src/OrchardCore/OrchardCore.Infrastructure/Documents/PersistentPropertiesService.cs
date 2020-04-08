using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level properties that are cached and persisted.
    /// </summary>
    public class PersistentPropertiesService : IPersistentPropertiesService
    {
        private readonly IDocumentManager<PersistentDocument> _documentManager;

        public PersistentPropertiesService(IDocumentManager<PersistentDocument> documentManager) => _documentManager = documentManager;

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string name) where T : new()
        {
            var document = await _documentManager.GetImmutableAsync();

            if (document.Properties.TryGetValue(name, out var value))
            {
                return value.ToObject<T>();
            }

            return new T();
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string name, T property) where T : new()
        {
            var document = await _documentManager.GetMutableAsync();

            document.Properties[name] = JObject.FromObject(property);

            await _documentManager.UpdateAsync(document);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string name)
        {
            var document = await _documentManager.GetMutableAsync();

            document.Properties.Remove(name);

            await _documentManager.UpdateAsync(document);
        }
    }

    public class PersistentDocument : DocumentEntity
    {
    }
}
