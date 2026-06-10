namespace OrchardCore.Media.Processing;

internal sealed class ImageProcessingResult : IDisposable
{
    public Stream Output { get; init; }
    public string ContentType { get; init; }
    public void Dispose() => Output?.Dispose();
}
