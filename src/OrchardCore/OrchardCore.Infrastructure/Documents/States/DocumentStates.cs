using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Documents.States
{
    /// <summary>
    /// Shares tenant level states.
    /// </summary>
    public class DocumentStates<TDocumentEntity> : IDocumentStates where TDocumentEntity : class, IDocumentEntity, new()
    {
        private readonly IDocumentManager<TDocumentEntity> _documentManager;

        public DocumentStates(IDocumentManager<TDocumentEntity> documentManager) => _documentManager = documentManager;

        public async Task<T> GetAsync<T>(string key) where T : new()
        {
            var document = await _documentManager.GetImmutableAsync();

            if (document.Properties.TryGetValue(key, out var value))
            {
                return value.ToObject<T>();
            }

            return new T();
        }

        public async Task SetAsync<T>(string key, T value) where T : new()
        {
            var document = await _documentManager.GetMutableAsync();
            document.Properties[key] = JObject.FromObject(value);
            await _documentManager.UpdateAsync(document);
        }

        public async Task RemoveAsync(string key)
        {
            var document = await _documentManager.GetMutableAsync();
            document.Properties.Remove(key);
            await _documentManager.UpdateAsync(document);
        }
    }
}
