using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Azure.Processing
{
    public class MediaBlobFileResolver : IImageResolver
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly string _filePath;
        private readonly ImageMetaData _metadata;

        public MediaBlobFileResolver(IMediaFileStore mediaStore, string filePath, in ImageMetaData metadata)
        {
            _mediaStore = mediaStore;
            _filePath = filePath;
            _metadata = metadata;
        }

        public MediaBlobFileResolver(IMediaFileStore mediaStore, in ImageMetaData metadata)
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
            // Check has already been done. be much better if we could just pass the BlobReference in
            //var file = await _mediaStore.GetFileInfoAsync(_filePath);

            //// Check to see if the file exists.
            //if (file == null)
            //{
            //    return null;
            //}

            return await _mediaStore.GetFileStreamAsync(_filePath);
        }
    }
}
