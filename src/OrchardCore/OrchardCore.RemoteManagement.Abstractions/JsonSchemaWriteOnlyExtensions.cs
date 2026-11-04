using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Reads the JSON Schema <c>writeOnly</c> keyword, which marks the values that are only ever sent to the server, such as secrets.
/// </summary>
public static class JsonSchemaWriteOnlyExtensions
{
    /// <summary>
    /// Gets whether the schema, or any schema nested in it, is marked <c>writeOnly</c>.
    /// </summary>
    /// <param name="schema">The JSON Schema to inspect.</param>
    /// <returns><see langword="true"/> when a <c>writeOnly</c> value is declared; otherwise, <see langword="false"/>.</returns>
    public static bool DeclaresWriteOnlyValue(this JsonNode schema) => schema switch
    {
        JsonObject values => values.Any(entry =>
            (entry.Key == "writeOnly" && entry.Value?.GetValueKind() == JsonValueKind.True) || DeclaresWriteOnlyValue(entry.Value)),
        JsonArray items => items.Any(DeclaresWriteOnlyValue),
        _ => false,
    };
}
