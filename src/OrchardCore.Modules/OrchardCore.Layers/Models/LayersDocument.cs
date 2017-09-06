using System.Collections.Generic;

namespace OrchardCore.Layers.Models
{
    public class LayersDocument
    {
        public int Id { get; set; }
        public List<Layer> Layers { get; set; } = new List<Layer>();
    }
}
