using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Media.Indexing
{
    /// <summary>
    /// Service for producing the textual representation of a media file, e.g. the text contained in a PDF document.
    /// </summary>
    public interface IMediaFileTextProvider
    {
        /// <summary>
        /// Produces the textual representation of the media file with the given path.
        /// </summary>
        /// <param name="path">The full relative path of the media file.</param>
        /// <param name="fileStream">The <see cref="Stream"/> that can be used to access the file's content.</param>
        /// <returns>The textual representation of the media file.</returns>
        Task<string> GetTextAsync(string path, Stream fileStream);
    }
}
