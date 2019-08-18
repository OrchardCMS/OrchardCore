namespace OrchardCore.Media
{
    /// <summary>
    /// Identifier for when a file provider serves as a cache for a remote file store.
    /// </summary>
    public interface IMediaCacheFileProvider : IMediaFileProvider
    {
        string BuildFilePath(string path);
    }
}
