using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.ViewModels
{
    public class EditGeoPointFieldViewModel
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        [BindNever]
        public GeoPointField Field { get; set; }

        [BindNever]
        public ContentPart Part { get; set; }

        [BindNever]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
