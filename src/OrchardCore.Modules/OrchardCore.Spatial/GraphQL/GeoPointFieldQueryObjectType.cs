using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.GraphQL;
public class GeoPointFieldQueryObjectType : ObjectGraphType<GeoPointField>
{
    public GeoPointFieldQueryObjectType(IStringLocalizer<GeoPointFieldQueryObjectType> S)
    {
        Name = nameof(GeoPointField);

        Field(x => x.Latitude, nullable: true)
            .Description(S["the latitude of the geo point"]);

        Field(x => x.Longitude, nullable: true)
            .Description(S["the longitude of the geo point"]);
    }
}
