using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using YesSql;

namespace OrchardCore.Layers.Services
{
    public class LayerService : ILayerService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;

        private const string LayersCacheKey = "LayersDocument";

        public LayerService(
            IMemoryCache memoryCache,
            ISignal signal,
            ISession session)
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
        }

        public async Task<LayersDocument> GetLayersAsync()
        {
            LayersDocument layers;

            if (!_memoryCache.TryGetValue(LayersCacheKey, out layers))
            {
                var changeToken = _signal.GetToken(LayersCacheKey);
                layers = await _session.Query<LayersDocument>().FirstOrDefaultAsync();

                if (layers == null)
                {
                    layers = new LayersDocument();

                    _session.Save(layers);
                    ShellScope.AddDeferredSignal(LayersCacheKey);
                }
                else
                {
                    _memoryCache.Set(LayersCacheKey, layers, changeToken);
                }
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
            var existing = await GetLayersAsync();
            existing.Layers = layers.Layers;

            _session.Save(existing);
            ShellScope.AddDeferredSignal(LayersCacheKey);
        }
    }
}
