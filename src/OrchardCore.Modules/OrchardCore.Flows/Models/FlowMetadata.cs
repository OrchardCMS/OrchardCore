using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models
{
    public enum FlowAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public class FlowMetadata : ContentPart
    {
        public FlowAlignment Alignment { get; set; } = FlowAlignment.Justify;
        public int Size { get; set; } = 100;
    }
}
