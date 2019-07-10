using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Services
{
    public class MediaFileProvider : PhysicalFileProvider, IMediaFileProvider
    {
        public MediaFileProvider(PathString virtualPathBase, string root, string cdnBaseUrl) : base(root)
        {
            VirtualPathBase = virtualPathBase;
            CdnBaseUrl = cdnBaseUrl;
        }

        public MediaFileProvider(PathString virtualPathBase, string root, string cdnBaseUrl, ExclusionFilters filters) : base(root, filters)
        {
            VirtualPathBase = virtualPathBase;
            CdnBaseUrl = cdnBaseUrl;
        }

        public PathString VirtualPathBase { get; }

        public string CdnBaseUrl { get; }
    }
}
