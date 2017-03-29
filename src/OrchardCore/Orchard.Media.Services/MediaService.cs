using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement;

namespace Orchard.Media
{
    public class MediaService : IMediaService
    {
        private readonly IEnumerable<IMediaFactorySelector> _mediaFactorySelectors;
        private readonly IMediaFileStore _mediaFileStore;

        public MediaService(IEnumerable<IMediaFactorySelector> mediaFactorySelectors, IMediaFileStore mediaFileStore)
        {
            _mediaFactorySelectors = mediaFactorySelectors;
            _mediaFileStore = mediaFileStore;
        }

        public async Task<IContent> ImportMediaAsync(string path, string mimeType, string contentType)
        {
            var file = await _mediaFileStore.GetFileAsync(path);

            if (file == null)
            {
                return null;
            }

            using (var stream = file.CreateReadStream())
            {
                var mediaFactory = GetMediaFactory(stream, file.Name, mimeType, contentType);

                if (mediaFactory == null)
                {
                    return null;
                }

                return mediaFactory.CreateMedia(stream, file.Path, mimeType, file.Length, contentType);
            }
        }

        public IMediaFactory GetMediaFactory(Stream stream, string fileName, string mimeType, string contentType)
        {
            var requestMediaFactoryResults = _mediaFactorySelectors
                .Select(x => x.GetMediaFactory(stream, fileName, mimeType, contentType))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .ToArray();

            if (requestMediaFactoryResults.Length == 0)
            {
                return null;
            }

            return requestMediaFactoryResults[0].MediaFactory;
        }
    }
}
