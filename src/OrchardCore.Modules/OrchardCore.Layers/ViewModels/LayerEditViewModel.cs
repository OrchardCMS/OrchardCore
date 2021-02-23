using System.Collections.Generic;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerEditViewModel
    {
        public bool AdminLayers { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public dynamic LayerRule { get; set; }
        public Dictionary<string, dynamic> Thumbnails { get; set; }
    }
}
