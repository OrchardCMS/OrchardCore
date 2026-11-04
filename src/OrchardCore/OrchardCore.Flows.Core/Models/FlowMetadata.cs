using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models;

public enum FlowAlignment
{
    Left,
    Center,
    Right,
    Justify,
    Inherit,
}

public class FlowMetadata : ContentPart
{
    [Description("The horizontal alignment of the flow item.")]
    public FlowAlignment Alignment { get; set; } = FlowAlignment.Justify;

    [Description("The width of the flow item as a percentage of its row.")]
    public int Size { get; set; } = 100;
}
