using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Services
{
    public class MediaFileProvider : PhysicalFileProvider, IMediaFileProvider, IMediaCacheFileProvider, IMediaFileStoreCacheProvider
    {
        public MediaFileProvider(PathString virtualPathBase, string root) : base(root)
        {
            VirtualPathBase = virtualPathBase;
        }

        public MediaFileProvider(PathString virtualPathBase, string root, ExclusionFilters filters) : base(root, filters)
        {
            VirtualPathBase = virtualPathBase;
        }

        public PathString VirtualPathBase { get; }
    }
}
