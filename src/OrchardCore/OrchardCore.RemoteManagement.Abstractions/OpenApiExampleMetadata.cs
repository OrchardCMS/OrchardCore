using System.Text.Json.Nodes;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Holds the request and response examples of an endpoint, for the OpenAPI document.
/// </summary>
public sealed class OpenApiExampleMetadata
{
    /// <summary>
    /// Creates the metadata from JSON documents.
    /// </summary>
    /// <param name="request">The JSON of the request example, or <see langword="null"/> when the operation has none.</param>
    /// <param name="response">The JSON of the example of the successful responses, or <see langword="null"/> when the operation has none.</param>
    public OpenApiExampleMetadata(string request, string response)
    {
        Request = request is null ? null : JsonNode.Parse(request);
        Response = response is null ? null : JsonNode.Parse(response);
    }

    /// <summary>
    /// Gets the example of the JSON request body.
    /// </summary>
    public JsonNode Request { get; }

    /// <summary>
    /// Gets the example of the JSON body of the successful responses.
    /// </summary>
    public JsonNode Response { get; }
}
