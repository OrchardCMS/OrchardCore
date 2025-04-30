using OrchardCore.FileStorage;

namespace OrchardCore.Media;

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

    /// <summary>
    /// Creates a new file in the file store from the contents of an input stream.
    /// </summary>
    /// <param name="path">The path of the file to be created.</param>
    /// <param name="inputStream">The stream whose contents to write to the new file.</param>
    /// <param name="overwrite"><c>true</c> to overwrite if a file already exists; <c>false</c> to throw an exception if the file already exists.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description><c>outputPath</c>: The final path of the created file within the file store.</description>
    /// </item>
    /// <item>
    /// <description><c>outputStream</c>: The resulting stream, which may be the original input stream or a modified stream if any event handlers altered it.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// If the specified path contains one or more directories, then those directories are
    /// created if they do not already exist.
    /// </remarks>
    public virtual Task<(string outputPath, Stream outputStream)> CreateMediaFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false) => Task.FromResult((path, inputStream));

}
