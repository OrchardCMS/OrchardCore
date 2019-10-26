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
        /// Returns all the layers for udpate.
        /// </summary>
        Task<LayersDocument> LoadLayersAsync();

        /// <summary>
        /// Returns all the layers in read-only.
        /// </summary>
        Task<LayersDocument> GetLayersAsync();

        Task<IEnumerable<ContentItem>> GetLayerWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
        Task<IEnumerable<LayerMetadata>> GetLayerWidgetsMetadataAsync(Expression<Func<ContentItemIndex, bool>> predicate);
        Task UpdateAsync(LayersDocument layers);
    }
}
