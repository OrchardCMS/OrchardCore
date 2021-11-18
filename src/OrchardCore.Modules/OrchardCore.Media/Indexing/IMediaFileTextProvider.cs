using System.IO;

namespace OrchardCore.Media.Indexing
{
    /// <summary>
    /// Service for producing the textual representation of a media file, e.g. the text contained in a PDF document.
    /// </summary>
    public interface IMediaFileTextProvider
    {
        /// <summary>
        /// Indicates if the implementation can produce the textual representation of the media file with the given
        /// path.
        /// </summary>
        /// <param name="path">The full relative path of the media file.</param>
        /// <returns>
        /// <see langword="true"/> if the implementation can produce the textual representation of the media file with
        /// the given path, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// A separate method is necessary since an empty or null return value from <see cref="GetText(string,
        /// Stream)"/> can simply mean the file's textual content is empty.
        /// </para>
        /// </remarks>
        bool CanHandle(string path);

        /// <summary>
        /// Produces the textual representation of the media file with the given path.
        /// </summary>
        /// <param name="path">The full relative path of the media file.</param>
        /// <param name="fileStream">The <see cref="Stream"/> that can be used to access the file's content.</param>
        /// <returns>The textual representation of the media file.</returns>
        string GetText(string path, Stream fileStream);
    }
}
