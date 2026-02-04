namespace OrchardCore.Media.Services;

/// <summary>
/// Provides the maximum file size for media uploads. If multiple providers exist, the first one (sorted by <see
/// cref="Order"/>) will be used that doesn't return <see langword="null"/>.
/// </summary>
public interface IMediaSizeLimitProvider
{
    /// <summary>
    /// Gets the order of execution. Providers are sorted by this value in ascending order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns the size limit in bytes, or <see langword="null"/> if this provider is not applicable.
    /// </summary>
    /// <returns></returns>
    Task<long?> GetMediaSizeLimitAsync();
}
