using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Layers.Models
{
    public class LayersDocument : Document
    {
        public List<Layer> Layers { get; set; } = new List<Layer>();
    }
}
