using OrchardCore.FileStorage;

namespace OrchardCore.Media
{
    /// <summary>
    /// Represents an abstraction over a specialized file store for storing media and service it to clients.
    /// </summary>
    public interface IMediaFileStore : IFileStore
    {
        /// <summary>
        /// Maps a path within the file store to a publicly accessible URL.
        /// </summary>
        /// <param name="path">The path within the file store.</param>
        /// <returns>A string containing the mapped public URL of the given path.</returns>
        string MapPathToPublicUrl(string path);
    }
}
