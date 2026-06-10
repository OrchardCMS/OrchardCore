namespace OrchardCore.Media;

public interface IResizedImageCache
{
    Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task ClearStaleAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}
