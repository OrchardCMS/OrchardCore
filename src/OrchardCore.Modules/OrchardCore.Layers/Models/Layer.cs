using OrchardCore.Rules.Models;

namespace OrchardCore.Layers.Models
{
    public class Layer
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public string Description { get; set; }

        public AllRule AllRule { get; set; } = new AllRule();
    }
}
