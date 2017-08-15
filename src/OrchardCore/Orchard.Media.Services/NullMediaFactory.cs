using System.IO;
using Orchard.ContentManagement;

namespace Orchard.Media
{
    public class NullMediaFactory : IMediaFactory
    {
        public static IMediaFactory Instance => new NullMediaFactory();

        public IContent CreateMedia(Stream stream, string path, string mimeType, long length, string contentType)
        {
            return null;
        }
    }
}
