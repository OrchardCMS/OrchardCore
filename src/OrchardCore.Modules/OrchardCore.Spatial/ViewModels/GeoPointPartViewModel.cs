using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.ViewModels
{
    public class GeoPointPartViewModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public GeoPointPart GeoPointPart { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}
