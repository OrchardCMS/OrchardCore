namespace OrchardCore.Media;

public interface IResizedImageCache
{
    /// <summary>
    /// Gets the cached resized image stored under the given key with the given file extension, or
    /// <see langword="null"/> when there is no matching cache entry. The extension is known ahead of
    /// time from the requested output format, so the cache performs a single deterministic lookup.
    /// </summary>
    /// <param name="cacheKey">The storage-agnostic cache key.</param>
    /// <param name="fileExtension">The expected file extension, including the leading dot (for example <c>.webp</c>).</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, string fileExtension, CancellationToken cancellationToken = default);

    Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);

    Task ClearStaleAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}
