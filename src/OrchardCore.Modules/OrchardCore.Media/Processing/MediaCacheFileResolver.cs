using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaCacheFileResolver : PhysicalFileSystemResolver, IMediaCacheFileResolver
    {
        public MediaCacheFileResolver(IFileInfo fileInfo, in ImageMetaData metadata) : base(fileInfo, metadata)
        {
            Length = fileInfo.Length;
            PhysicalPath = fileInfo.PhysicalPath;
        }

        public long Length { get; set; }

        public string PhysicalPath { get; set; }
    }
}
