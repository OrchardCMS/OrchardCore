using OrchardCore.Rules;

namespace OrchardCore.Layers.Models;

public class Layer
{
    public string Name { get; set; }

    public string Description { get; set; }

    public Rule LayerRule { get; set; }
}
