using System.Collections.Immutable;

namespace OrchardCore.Layers.Models
{
    public class LayersDocument
    {
        public int Id { get; set; }
        public ImmutableArray<Layer> Layers { get; set; } = ImmutableArray.Create<Layer>();
    }
}
