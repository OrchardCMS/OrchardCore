using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Spatial.Fields;

public class GeoPointField : ContentField
{
    [Description("The latitude in decimal degrees from -90 to 90, or null when unset.")]
    public decimal? Latitude { get; set; }

    [Description("The longitude in decimal degrees from -180 to 180, or null when unset.")]
    public decimal? Longitude { get; set; }
}
