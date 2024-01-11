using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocumentEntity"/> between an <see cref="IDocumentStore"/> and a multi level cache.
    /// </summary>
    public class DocumentEntityManager<TDocumentEntity> : IDocumentEntityManager<TDocumentEntity> where TDocumentEntity : class, IDocumentEntity, new()
    {
        private readonly IDocumentManager<TDocumentEntity> _documentManager;

        public DocumentEntityManager(IDocumentManager<TDocumentEntity> documentManager) => _documentManager = documentManager;

        public async Task<T> GetAsync<T>(string key) where T : new()
        {
            var document = await _documentManager.GetOrCreateImmutableAsync();

            if (document.Properties.TryGetValue(key, out var value))
            {
                return value.ToObject<T>();
            }

            return new T();
        }

        public async Task SetAsync<T>(string key, T value) where T : new()
        {
            var document = await _documentManager.GetOrCreateMutableAsync();
            document.Properties[key] = JObject.FromObject(value);
            await _documentManager.UpdateAsync(document);
        }

        public async Task RemoveAsync(string key)
        {
            var document = await _documentManager.GetOrCreateMutableAsync();
            document.Properties.Remove(key);
            await _documentManager.UpdateAsync(document);
        }
    }
}
