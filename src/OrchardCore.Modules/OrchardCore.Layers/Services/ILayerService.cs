using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Services
{
    public interface ILayerService
    {
        /// <summary>
        /// Loads the layers document from the store for updating and that should not be cached.
        /// </summary>
        Task<LayersDocument> LoadLayersAsync();

        /// <summary>
        /// Gets the layers document from the cache for sharing and that should not be updated.
        /// </summary>
        Task<LayersDocument> GetLayersAsync();

        Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
        Task<IEnumerable<LayerMetadata>> GetLayerWidgetsMetadataAsync(Expression<Func<ContentItemIndex, bool>> predicate);

        /// <summary>
        /// Updates the store with the provided layers document and then updates the cache.
        /// </summary>
        Task UpdateAsync(LayersDocument layers);
    }
}
