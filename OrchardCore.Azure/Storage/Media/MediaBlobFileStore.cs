using Microsoft.Extensions.Options;
using OrchardCore.Media;

namespace OrchardCore.Azure.Storage.Media
{
    public class MediaBlobFileStore : BlobFileStore, IMediaFileStore
    {
        public MediaBlobFileStore(IOptionsSnapshot<MediaBlobStorageOptions> options) : base(options.Value)
        {
        }
    }
}
