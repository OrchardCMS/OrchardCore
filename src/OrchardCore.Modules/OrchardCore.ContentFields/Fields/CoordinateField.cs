using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class CoordinateField : ContentField
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}