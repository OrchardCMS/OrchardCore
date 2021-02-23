using OrchardCore.ContentManagement;

namespace OrchardCore.Layers.Models
{
    public interface ILayerMetadata
    {
        double Position { get; set; }
        string Zone { get; set; }
        string Layer { get; set; }
        ContentItem ContentItem { get; set; }
    }

    public class AdminLayerMetadata : ContentPart, ILayerMetadata
    {
        public double Position { get; set; }
        public string Zone { get; set; }
        public string Layer { get; set; }
    }
}
