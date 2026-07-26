namespace OrchardCore.Media.Processing;

/// <summary>
/// The encoded output of an <see cref="IImageProcessingEngine"/> transform.
/// </summary>
public sealed class ImageProcessingResult : IDisposable
{
    /// <summary>
    /// Gets the encoded image stream. The consumer owns this stream and must dispose it.
    /// </summary>
    public Stream Output { get; init; }

    /// <summary>
    /// Gets the content type (MIME type) of the encoded image.
    /// </summary>
    public string ContentType { get; init; }

    public void Dispose() => Output?.Dispose();
}
