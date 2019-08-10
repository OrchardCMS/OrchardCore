using OrchardCore.FileStorage;

namespace OrchardCore.Media
{
    /// <summary>
    /// Provides path mappings to map from a virtual path to a public path.
    /// </summary>
    public interface IMediaFileStorePathProvider : ICdnPathProvider
    {
        /// <summary>
        /// Maps a path within the file store to a publicly accessible URL.
        /// </summary>
        /// <param name="path">The path within the file store.</param>
        /// <returns>A string containing the mapped public URL of the given path.</returns>
        string MapPathToPublicUrl(string path);

        /// <summary>
        /// Maps a public URL to a path within the file store.
        /// </summary>
        /// <param name="publicPath">The public URL to map.</param>
        /// <returns>The mapped path of the given public URL within the file store.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the specified <paramref name="requestPath"/> value is not within the publicly accessible URL space of the file store.</exception>
        string MapPublicUrlToPath(string publicPath);
    }
}
