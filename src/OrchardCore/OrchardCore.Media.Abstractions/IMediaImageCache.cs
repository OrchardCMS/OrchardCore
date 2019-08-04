using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;

namespace OrchardCore.Media
{
    /// <summary>
    /// A cache for media assets, that supports a simpler locking mechanism,
    /// and cache clearing.
    /// </summary>
    public interface IMediaImageCache : IImageCache
    {
        /// <summary>
        /// Try to set a cache file without ImageMetadata.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="stream"></param>
        Task TrySetAsync(string cacheFilePath, Stream stream);

        //TODO add cache clear
    }
}
