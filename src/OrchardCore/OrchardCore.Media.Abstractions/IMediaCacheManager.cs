using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Media
{
    public interface IMediaCacheManager
    {
        /// <summary>
        /// Clears the media cache folder of all contents.
        /// </summary>
        Task<bool> ClearMediaCacheAsync(string cacheName);

        /// <summary>
        /// Gets a model for each registered cache management provider.
        /// </summary>
        IEnumerable<dynamic> GetCaches();
    }
}
