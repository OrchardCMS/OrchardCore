using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileResolver : IImageResolver
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly string _filePath;
        private readonly ImageMetaData _metadata;

        public MediaFileResolver(IMediaFileStore mediaStore, string filePath, in ImageMetaData metadata)
        {
            _mediaStore = mediaStore;
            _filePath = filePath;
            _metadata = metadata;
        }

        public MediaFileResolver(IMediaFileStore mediaStore, in ImageMetaData metadata)
        {
            _mediaStore = mediaStore;
            _metadata = metadata;
        }

        /// <inheritdoc/>
        public Task<ImageMetaData> GetMetaDataAsync()
        {
            return Task.FromResult(_metadata);
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
