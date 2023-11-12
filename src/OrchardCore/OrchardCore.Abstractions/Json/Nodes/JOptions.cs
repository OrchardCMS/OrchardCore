using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace System.Text.Json.Nodes;

/// <summary>
/// Centralizes common <see cref="JsonSerializerOptions" /> instances.
/// </summary>
public static class JOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        WriteIndented = false,
    };

    public static readonly JsonSerializerOptions Indented;
    public static readonly JsonSerializerOptions CamelCase;
    public static readonly JsonSerializerOptions CamelCaseIndented;
    public static readonly JsonSerializerOptions UnsafeRelaxedJsonEscaping;
    public static readonly JsonDocumentOptions Document;
    public static readonly JsonNodeOptions Node;

    static JOptions()
    {
        Default.Converters.Add(new JsonDynamicConverter());

        Indented = new JsonSerializerOptions(Default)
        {
            WriteIndented = true,
        };

        CamelCase = new JsonSerializerOptions(Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        CamelCaseIndented = new JsonSerializerOptions(Indented)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        UnsafeRelaxedJsonEscaping = new JsonSerializerOptions(Default)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        Document = new JsonDocumentOptions
        {
            CommentHandling = Default.ReadCommentHandling,
            AllowTrailingCommas = Default.AllowTrailingCommas,
        };

        Node = new JsonNodeOptions
        {
            PropertyNameCaseInsensitive = Default.PropertyNameCaseInsensitive,
        };
    }
}
