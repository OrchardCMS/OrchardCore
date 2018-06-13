using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.ViewModels
{
    public class DisplayCoordinateFieldViewModel
    {
        public CoordinateField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
