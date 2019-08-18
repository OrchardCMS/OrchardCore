namespace OrchardCore.Media
{
    /// <summary>
    /// Identifier for when a file provider serves as a cache provider.
    /// </summary>
    public interface IMediaCacheFileProvider : IMediaFileProvider
    {
        string Root { get; }
    }
}
