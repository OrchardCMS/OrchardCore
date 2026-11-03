namespace OrchardCore.RemoteManagement;

/// <summary>
/// Marks an endpoint as returning the version of its resource in the <c>ETag</c> header, for the OpenAPI document.
/// </summary>
public sealed class ResourceVersionMetadata
{
    /// <summary>
    /// Gets whether the endpoint also accepts the optional <c>If-Match</c> header.
    /// </summary>
    public bool AcceptsIfMatch { get; init; }
}
