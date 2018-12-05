using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileResolver : IImageResolver
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly string _filePath;

        public MediaFileResolver(IMediaFileStore mediaStore, string filePath)
        {
            _mediaStore = mediaStore;
            _filePath = filePath;
        }

        /// <inheritdoc/>
        public async Task<DateTime> GetLastWriteTimeUtcAsync()
        {
            var file = await _mediaStore.GetFileInfoAsync(_filePath);
            return file.LastModifiedUtc;
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenReadAsync()
        {
            var file = await _mediaStore.GetFileInfoAsync(_filePath);

            // Check to see if the file exists.
            if (file == null)
            {
                return null;
            }

            return await _mediaStore.GetFileStreamAsync(_filePath);
        }
    }
}
