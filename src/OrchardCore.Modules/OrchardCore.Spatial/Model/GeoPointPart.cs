using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.Model
{
    public class GeoPointPart : ContentPart
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
