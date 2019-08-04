using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Web.Caching;

namespace OrchardCore.Media
{
    //TODO add cache clear
    public interface IShellImageCache : IImageCache
    {
        /// <summary>
        /// Try to set a cache file without ImageMetadata.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="stream"></param>
        Task TrySetAsync(string cacheFilePath, Stream stream);
    }
}
