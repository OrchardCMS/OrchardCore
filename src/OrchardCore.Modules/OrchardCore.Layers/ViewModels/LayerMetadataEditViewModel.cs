using System.Collections.Generic;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerMetadataEditViewModel
    {
        public LayerMetadata LayerMetadata { get; set; }
		public List<Layer> Layers { get; set; }
    }
}
