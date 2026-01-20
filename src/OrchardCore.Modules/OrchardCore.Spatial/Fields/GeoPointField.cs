using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.Fields
{
    public class GeoPointField : ContentField
    {
        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
