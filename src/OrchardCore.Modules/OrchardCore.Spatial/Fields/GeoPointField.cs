using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.Fields
{
    public class GeoPointField : ContentField
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
