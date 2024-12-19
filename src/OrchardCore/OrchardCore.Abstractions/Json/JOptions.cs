using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// Centralizes common <see cref="JsonSerializerOptions" /> instances.
/// </summary>
public static class JOptions
{
    public static readonly JsonConverter[] KnownConverters =
    [
        DynamicJsonConverter.Instance,
        PathStringJsonConverter.Instance,
        TimeSpanJsonConverter.Instance,
        DateTimeJsonConverter.Instance,
        new JsonStringEnumConverter()
    ];

    public static readonly JsonSerializerOptions Base = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        ReferenceHandler = null, // Needed by JsonObjectCreationHandling.Populate.
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        WriteIndented = false,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static readonly JsonSerializerOptions Default;
    public static readonly JsonSerializerOptions Indented;
    public static readonly JsonSerializerOptions CamelCase;
    public static readonly JsonSerializerOptions CamelCaseIndented;
    public static readonly JsonSerializerOptions UnsafeRelaxedJsonEscaping;

    public static readonly JsonNodeOptions Node;
    public static readonly JsonDocumentOptions Document;

    static JOptions()
    {
        Default = new JsonSerializerOptions(Base);

        foreach (var converter in KnownConverters)
        {
            Default.Converters.Add(converter);
        }

        Indented = new JsonSerializerOptions(Default)
        {
            WriteIndented = true,
        };

        CamelCase = new JsonSerializerOptions(Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        CamelCaseIndented = new JsonSerializerOptions(CamelCase)
        {
            WriteIndented = true,
        };

        UnsafeRelaxedJsonEscaping = new JsonSerializerOptions(Default)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        Node = new JsonNodeOptions
        {
            PropertyNameCaseInsensitive = Default.PropertyNameCaseInsensitive,
        };

        Document = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
    }
}
