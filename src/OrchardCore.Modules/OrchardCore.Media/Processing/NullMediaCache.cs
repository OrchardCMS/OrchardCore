using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// A null media cache, for use when a CDN will be handling caching.
    /// </summary>
    public class NullMediaCache : IMediaImageCache
    {
        [Obsolete("This feature is unused and has been removed from ImageSharp.Web, remove when updating ImageSharp.Web.")]
        public IDictionary<string, string> Settings => throw new NotImplementedException();

        public Task<IImageResolver> GetAsync(string key)
        {
            // This must return a null Task or it will throw a NullReferenceException.
            return Task.FromResult<IImageResolver>(null);
        }

        public Task<IMediaCacheFileResolver> GetMediaCacheFileAsync(string key)
        {
            return Task.FromResult<IMediaCacheFileResolver>(null);
        }

        public Task SetAsync(string key, Stream stream, ImageMetaData metadata)
        {
            return Task.CompletedTask;
        }

        public Task TrySetAsync(string key, string extension, Stream stream, ImageMetaData metadata)
        {
            return Task.CompletedTask;
        }
    }
}
