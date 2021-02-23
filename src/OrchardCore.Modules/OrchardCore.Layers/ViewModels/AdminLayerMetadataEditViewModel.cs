using System.Collections.Generic;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.ViewModels
{
    public class AdminLayerMetadataEditViewModel
    {
        public string Title { get; set; }
        public AdminLayerMetadata LayerMetadata { get; set; }
        public List<Layer> Layers { get; set; }
    }
}
