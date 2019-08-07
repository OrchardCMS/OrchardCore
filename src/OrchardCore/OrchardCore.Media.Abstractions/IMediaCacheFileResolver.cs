using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media
{
    /// <summary>
    /// A media cache resolver for use with a physical file cache.
    /// </summary>
    public interface IMediaCacheFileResolver : IImageResolver
    {
        /// <summary>
        /// File length.
        /// </summary>
        long Length { get; set; }

        /// <summary>
        /// Physical path of file.
        /// </summary>
        string PhysicalPath { get; set; }
    }
}
