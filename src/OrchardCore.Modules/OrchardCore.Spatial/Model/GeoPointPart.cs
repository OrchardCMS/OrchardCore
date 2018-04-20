using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial
{
    public class GeoPointPart : ContentPart
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
