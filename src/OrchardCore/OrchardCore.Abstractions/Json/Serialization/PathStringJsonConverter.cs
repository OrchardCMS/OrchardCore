using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Json.Serialization;

public class PathStringJsonConverter : JsonConverter<PathString>
{
    public static readonly PathStringJsonConverter Instance = new();

    public override PathString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString());

    public override void Write(Utf8JsonWriter writer, PathString value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, writer.ToString(), typeof(string), options);
}
