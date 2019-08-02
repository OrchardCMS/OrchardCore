using System.IO;
using System.Threading.Tasks;
using OrchardCore.FileStorage.AzureBlob;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Azure.Processing
{
    public class MediaBlobFileResolver : IImageResolver
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly BlobFile _fileStoreEntry;
        private readonly ImageMetaData _metadata;

        public MediaBlobFileResolver(IMediaFileStore mediaStore, BlobFile fileStoreEntry, in ImageMetaData metadata)
        {
            _mediaStore = mediaStore;
            _fileStoreEntry = fileStoreEntry;
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
            // Check to see if the file exists.
            if (_fileStoreEntry == null)
            {
                return null;
            }

            return await _mediaStore.GetFileStreamAsync(_fileStoreEntry);
        }
    }
}
