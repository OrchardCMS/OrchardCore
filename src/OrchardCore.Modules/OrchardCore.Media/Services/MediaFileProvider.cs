using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Services
{
    public class MediaFileProvider : PhysicalFileProvider, IMediaFileProvider
    {
        public string VirtualPathBase { get; set; }
        public MediaFileProvider(string virtualPathBase, string root) : base(root)
        {
            VirtualPathBase = virtualPathBase;
        }
        public MediaFileProvider(string virtualPathBase, string root, ExclusionFilters filters) : base(root, filters)
        {
            VirtualPathBase = virtualPathBase;
        }
    }
}
