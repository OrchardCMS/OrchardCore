using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Layers.Models;

public class LayerMetadata : ContentPart
{
    [Description("Whether the widget's title is rendered.")]
    public bool RenderTitle { get; set; }

    [Description("The widget's ordering position within its zone.")]
    public double Position { get; set; }

    [Description("The name of the layout zone where the widget is rendered.")]
    public string Zone { get; set; }

    [Description("The name of the layer that controls the widget's visibility.")]
    public string Layer { get; set; }
}
