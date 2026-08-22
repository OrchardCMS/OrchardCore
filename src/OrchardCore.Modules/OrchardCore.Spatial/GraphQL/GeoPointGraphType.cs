using System.Text.Json;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.GraphQL;

public class GeoPointGraphType : ScalarGraphType
{
    public GeoPointGraphType(IStringLocalizer<GeoPointGraphType> S)
    {
        Name = "GeoPoint";
        Description = S["Represent a geo location with latitude and longitude."];
    }

    public override object Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }
    public override object ParseValue(object value)
    {
        var location = value?.ToString();
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GeoPointField>(location);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
