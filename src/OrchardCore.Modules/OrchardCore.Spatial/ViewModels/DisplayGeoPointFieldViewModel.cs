using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.ViewModels
{
    public class DisplayGeoPointFieldViewModel
    {
        public GeoPointField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
