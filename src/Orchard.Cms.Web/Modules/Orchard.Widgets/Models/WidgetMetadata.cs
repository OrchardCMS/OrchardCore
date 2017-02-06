using Orchard.ContentManagement;

namespace Orchard.Widgets.Models
{
    public class WidgetMetadata : ContentPart
    {
        public bool RenderTitle { get; set; }
        public string Position { get; set; }
    }
}
