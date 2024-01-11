using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Documents;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using YesSql;

namespace OrchardCore.Layers.Services
{
    public class LayerService : ILayerService
    {
        private readonly ISession _session;
        private readonly IDocumentManager<LayersDocument> _documentManager;

        public LayerService(ISession session, IDocumentManager<LayersDocument> documentManager)
        {
            _session = session;
            _documentManager = documentManager;
        }

        /// <summary>
        /// Loads the layers document from the store for updating and that should not be cached.
        /// </summary>
        public Task<LayersDocument> LoadLayersAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the layers document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<LayersDocument> GetLayersAsync() => _documentManager.GetOrCreateImmutableAsync();

        public Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(
            Expression<Func<ContentItemIndex, bool>> predicate)
        {
            return _session
                .Query<ContentItem, LayerMetadataIndex>()
                .With(predicate)
                .ListAsync();
        }

        public async Task<IEnumerable<LayerMetadata>> GetLayerWidgetsMetadataAsync(
            Expression<Func<ContentItemIndex, bool>> predicate)
        {
            var allWidgets = await GetLayerWidgetsAsync(predicate);

            return allWidgets
                .Select(x => x.As<LayerMetadata>())
                .Where(x => x != null)
                .OrderBy(x => x.Position)
                .ToList();
        }

        /// <summary>
        /// Updates the store with the provided layers document and then updates the cache.
        /// </summary>
        public async Task UpdateAsync(LayersDocument layers)
        {
            var existing = await LoadLayersAsync();
            existing.Layers = layers.Layers;
            await _documentManager.UpdateAsync(layers);
        }
    }
}
