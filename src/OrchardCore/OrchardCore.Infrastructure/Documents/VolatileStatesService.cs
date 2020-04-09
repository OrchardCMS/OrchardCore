using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level states that are cached but not persisted.
    /// </summary>
    public class VolatileStatesService : IVolatileStates
    {
        private readonly IVolatileDocumentManager<VolatileDocument> _documentManager;

        public VolatileStatesService(IVolatileDocumentManager<VolatileDocument> documentManager) => _documentManager = documentManager;

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key) where T : new()
        {
            var document = await _documentManager.GetImmutableAsync();

            if (document.Properties.TryGetValue(key, out var value))
            {
                return value.ToObject<T>();
            }

            return new T();
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value) where T : new()
        {
            var document = await _documentManager.GetMutableAsync();
            document.Properties[key] = JObject.FromObject(value);
            await _documentManager.UpdateAsync(document);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key)
        {
            var document = await _documentManager.GetMutableAsync();
            document.Properties.Remove(key);
            await _documentManager.UpdateAsync(document);
        }
    }

    public class VolatileDocument : DocumentEntity
    {
    }
}
