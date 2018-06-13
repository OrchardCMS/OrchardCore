using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.Fields
{
    public class CoordinateField : ContentField
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}