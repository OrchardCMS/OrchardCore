using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Services
{
    public class MediaCacheFileProvider : PhysicalFileProvider, IMediaCacheFileProvider
    {
        public MediaCacheFileProvider(string root) : base(root) { }

        public MediaCacheFileProvider(string root, ExclusionFilters filters) : base(root, filters) { }
    }
}
