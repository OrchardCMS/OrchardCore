namespace OrchardCore.Media
{
    /// <summary>
    /// Identifier for when a file provider serves as a cache provider for a file store.
    /// </summary>
    public interface IMediaFileStoreCacheFileProvider : IMediaFileProvider, IMediaFileStoreCache
    {
    }
}
