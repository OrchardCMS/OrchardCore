using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media
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
            var file = await _mediaFileStore.GetFileInfoAsync(path);

            if (file == null)
            {
                return null;
            }

            using (var stream = await _mediaFileStore.GetFileStreamAsync(path))
            {
                var mediaFactory = await GetMediaFactoryAsync(stream, file.Name, mimeType, contentType);

                if (mediaFactory == null)
                {
                    return null;
                }

                return await mediaFactory.CreateMediaAsync(stream, file.Path, mimeType, file.Length, contentType);
            }
        }

        public async Task<IMediaFactory> GetMediaFactoryAsync(Stream stream, string fileName, string mimeType, string contentType)
        {
            var results = new List<MediaFactorySelectorResult>();
            foreach (var selector in _mediaFactorySelectors)
            {
                var result = await selector.GetMediaFactoryAsync(stream, fileName, mimeType, contentType);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            var requestMediaFactoryResult = results
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

            return requestMediaFactoryResult?.MediaFactory;
        }
    }
}
