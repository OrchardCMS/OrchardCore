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
		private readonly IMemoryCache _memoryCache;
		private readonly ISession _session;
		private readonly ISignal _signal;

		private const string LayersCacheKey = "LayersDocument";

		public LayerService(
			ISignal signal,
			ISession session,
			IMemoryCache memoryCache)
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
				layers = await _session.Query<LayersDocument>().FirstOrDefaultAsync();

				if (layers == null)
				{
					lock (_memoryCache)
					{
						if (!_memoryCache.TryGetValue(LayersCacheKey, out layers))
						{
							layers = new LayersDocument();

							_session.Save(layers);
							_memoryCache.Set(LayersCacheKey, layers);
						}
					}
				}
				else
				{
					_memoryCache.Set(LayersCacheKey, layers);
				}
			}

			return layers;
		}

		public async Task<IEnumerable<LayerMetadata>> GetLayerWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate)
		{
            var allWidgets = await _session
                .Query<ContentItem, LayerMetadataIndex>()
                .With(predicate)
                .ListAsync();

            return allWidgets
                .Select(x => x.As<LayerMetadata>())
                .Where(x => x != null)
                .OrderBy(x => x.Position)
                .ToList();

		}

		public async Task UpdateAsync(LayersDocument layers)
		{
			var existing = await _session.Query<LayersDocument>().FirstOrDefaultAsync();

			existing.Layers = layers.Layers;
			_session.Save(existing);
			_memoryCache.Set(LayersCacheKey, existing);
		}
	}
}
