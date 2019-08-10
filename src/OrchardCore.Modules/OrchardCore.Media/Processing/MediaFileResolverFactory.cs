using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileResolverFactory : IMediaFileResolverFactory
    {
        private readonly IMediaFileStore _mediaStore;

        public MediaFileResolverFactory(IMediaFileStore mediaStore)
        {
            _mediaStore = mediaStore;
        }

        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Path has already been correctly parsed before here.
            var filePath = _mediaStore.MapPublicUrlToPath(context.Request.PathBase + context.Request.Path.Value);

            // Check to see if the file exists.
            var file = await _mediaStore.GetFileInfoAsync(filePath);
            if (file == null)
            {
                return null;
            }
            var metadata = new ImageMetaData(file.LastModifiedUtc);
            return new MediaFileResolver(_mediaStore, filePath, metadata);
        }
    }
}
