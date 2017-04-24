using System.Collections.Generic;
using Orchard.Layers.Models;

namespace Orchard.Layers.ViewModels
{
    public class LayerMetadataEditViewModel
    {
        public LayerMetadata LayerMetadata { get; set; }
		public List<Layer> Layers { get; set; }
    }
}
