using System.Collections.Generic;

namespace Orchard.Layers.Models
{
    public class LayersDocument
    {
        public List<Layer> Layers { get; set; } = new List<Layer>();
    }
}
