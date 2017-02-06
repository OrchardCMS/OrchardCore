using Orchard.ContentManagement;

namespace Orchard.Layers.Models
{
    public class LayerMetadata : ContentPart
    {
        public string Title { get; set; }
        public bool RenderTitle { get; set; }
        public double Position { get; set; }
		public string Zone { get; set; }
		public string Layer { get; set; }
	}
}
