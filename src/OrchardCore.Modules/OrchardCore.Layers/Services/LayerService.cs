using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Cache;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using YesSql;

namespace OrchardCore.Layers.Services
{
    public class LayerService : ILayerService
    {
        private const string LayersCacheKey = "LayersDocument";

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;

        private LayersDocument _layersDocument;

        public LayerService(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<LayersDocument> LoadLayersAsync()
        {
            return _layersDocument ??= await _session.Query<LayersDocument>().FirstOrDefaultAsync() ?? new LayersDocument();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        /// <inheritdoc/>

        public async Task<LayersDocument> GetLayersAsync()
        {
            if (!_memoryCache.TryGetValue<LayersDocument>(LayersCacheKey, out var layers))
            {
                var changeToken = _signal.GetToken(LayersCacheKey);

                if (_layersDocument != null)
                {
                    _session.Detach(_layersDocument);
                }

                layers = await _session.Query<LayersDocument>().FirstOrDefaultAsync();

                if (layers != null)
                {
                    _session.Detach(layers);
                }
                else
                {
                    layers = new LayersDocument();
                }

                layers.IsReadonly = true;

                _memoryCache.Set(LayersCacheKey, layers, changeToken);
            }

            return layers;
        }

        public async Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(
            Expression<Func<ContentItemIndex, bool>> predicate)
        {
            return await _session
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

        public async Task UpdateAsync(LayersDocument layers)
        {
            if (layers.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var existing = await LoadLayersAsync();
            existing.Layers = layers.Layers;

            _session.Save(existing);
            _signal.DeferredSignalToken(LayersCacheKey);
        }
    }
}
