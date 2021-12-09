using System.Collections.Generic;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerMetadataEditViewModel
    {
        public string Title { get; set; }
        public LayerMetadata LayerMetadata { get; set; }
        public List<Layer> Layers { get; set; }
    }
}
