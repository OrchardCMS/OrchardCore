using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Azure.Services
{
    public class MediaBlobFileProvider : PhysicalFileProvider, IMediaFileProvider, IMediaCacheFileProvider
    {
        public MediaBlobFileProvider(PathString virtualPathBase, string root) : base(root)
        {
            VirtualPathBase = virtualPathBase;
        }

        public MediaBlobFileProvider(PathString virtualPathBase, string root, ExclusionFilters filters) : base(root, filters)
        {
            VirtualPathBase = virtualPathBase;
        }

        public PathString VirtualPathBase { get; }

        public string BuildFilePath(string path)
        {
            return Path.Combine(Root, path.Substring(1));
        }
    }
}
