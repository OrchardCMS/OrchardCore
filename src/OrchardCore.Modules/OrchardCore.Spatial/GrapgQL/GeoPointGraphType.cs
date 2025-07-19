using System.Text.Json;
using GraphQL.Types;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.GrapgQL;
public class GeoPointGraphType : ScalarGraphType
{
    public GeoPointGraphType()
    {
        Name = "GeoPoint";
        Description = "Representa a geo location with latitude and longitude.";
    }

    public override object Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }
    public override object? ParseValue(object? value)
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
