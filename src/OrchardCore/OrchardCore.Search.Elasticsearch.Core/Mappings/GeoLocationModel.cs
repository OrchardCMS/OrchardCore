using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Mappings;

internal sealed class GeoLocationModel
{
    [Number(Name = "Latitude")]
    public string Latitude { get; set; }

    [Number(Name = "Longitude")]
    public string Longitude { get; set; }
}
