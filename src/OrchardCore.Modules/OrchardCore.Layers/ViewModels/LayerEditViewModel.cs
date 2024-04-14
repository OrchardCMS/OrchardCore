using System.Collections.Generic;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerEditViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public dynamic LayerRule { get; set; }
        public Dictionary<string, dynamic> Thumbnails { get; set; }
    }
}
