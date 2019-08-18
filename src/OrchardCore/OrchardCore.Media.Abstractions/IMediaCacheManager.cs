using System.Threading.Tasks;

namespace OrchardCore.Media
{
    public interface IMediaCacheManager
    {
        /// <summary>
        /// Clears the media cache folder of all contents.
        /// </summary>
        Task<bool> ClearMediaCacheAsync();
    }
}
