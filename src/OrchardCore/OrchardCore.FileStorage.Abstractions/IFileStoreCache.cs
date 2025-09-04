namespace OrchardCore.FileStorage;

/// <summary>
/// Cache a file store.
/// </summary>
public interface IFileStoreCache
{
    Task<bool> IsCachedAsync(string path);
    Task SetCacheAsync(Stream stream, IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken);
}
