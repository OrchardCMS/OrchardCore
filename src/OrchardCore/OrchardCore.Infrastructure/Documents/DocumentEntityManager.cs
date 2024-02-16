using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocumentEntity"/> between an <see cref="IDocumentStore"/> and a multi level cache.
    /// </summary>
    public class DocumentEntityManager<TDocumentEntity> : IDocumentEntityManager<TDocumentEntity> where TDocumentEntity : class, IDocumentEntity, new()
    {
        private readonly IDocumentManager<TDocumentEntity> _documentManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DocumentEntityManager(IDocumentManager<TDocumentEntity> documentManager,
             IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _documentManager = documentManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task<T> GetAsync<T>(string key) where T : new()
        {
            var document = await _documentManager.GetOrCreateImmutableAsync();
            if (document.Properties.TryGetPropertyValue(key, out var value))
            {
                return value.Deserialize<T>(_jsonSerializerOptions);
            }

            return new T();
        }

        public async Task SetAsync<T>(string key, T value) where T : new()
        {
            var document = await _documentManager.GetOrCreateMutableAsync();
            document.Properties[key] = JObject.FromObject(value, _jsonSerializerOptions);
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
