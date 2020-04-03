using System.IO;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media
{
    public interface IMediaService
    {
        /// <summary>
        /// Import an existing file from the media store as a media content item
        /// </summary>
        /// <param name="path">The path of the file to import.</param>
        /// <param name="mimeType">The mime type of the file.</param>
        /// <param name="contentType">The content type to create from the file.</param>
        /// <returns>The created content item, or <c>null</c> if none could be created.</returns>
        Task<IContent> ImportMediaAsync(string path, string mimeType, string contentType);

        /// <summary>
        /// Gets the <see cref="IMediaFactory"/> instance to process a file stream.
        /// </summary>
        /// <param name="stream">The stream of the file to process.</param>
        /// <param name="fileName">The name of the file to process.</param>
        /// <param name="mimeType">The mime-type of the file to process.</param>
        /// <param name="contentType">The content type intended to be created.</param>
        /// <returns></returns>
        IMediaFactory GetMediaFactory(Stream stream, string fileName, string mimeType, string contentType);
    }
}
