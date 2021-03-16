using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Sitemaps.Cache
{
    public class PhysicalSitemapCacheFileResolver : ISitemapCacheFileResolver
    {
        private readonly IFileInfo _fileInfo;

        public PhysicalSitemapCacheFileResolver(IFileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public Task<Stream> OpenReadStreamAsync()
        {
            return Task.FromResult(_fileInfo.CreateReadStream());
        }
    }
}
