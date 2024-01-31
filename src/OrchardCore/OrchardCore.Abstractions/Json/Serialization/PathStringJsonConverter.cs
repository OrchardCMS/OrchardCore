using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Json.Serialization;

public class PathStringJsonConverter : JsonConverter<PathString>
{
    public static readonly PathStringJsonConverter Instance = new();

    public override PathString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = reader.GetString();

        if (string.IsNullOrEmpty(result))
        {
            return PathString.Empty;
        }

        if (!result.StartsWith('/'))
        {
            return new PathString('/' + result);
        }

        return new PathString(result);
    }

    public override void Write(Utf8JsonWriter writer, PathString value, JsonSerializerOptions options)
    {
        // https://localhost:44300/blog1/Admin/Settings/general

        JsonSerializer.Serialize(writer, value.ToString(), typeof(string), options);


    }
}
