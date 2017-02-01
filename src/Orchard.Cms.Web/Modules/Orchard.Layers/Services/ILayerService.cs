using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orchard.ContentManagement.Records;
using Orchard.Layers.Models;

namespace Orchard.Layers.Services
{
    public interface ILayerService
    {
		Task<LayersDocument> GetLayersAsync();
		Task<IEnumerable<LayerMetadata>> GetLayerWidgetsAsync(Expression<Func<ContentItemIndex, bool>> predicate);
		Task UpdateAsync(LayersDocument layers);
	}
}
