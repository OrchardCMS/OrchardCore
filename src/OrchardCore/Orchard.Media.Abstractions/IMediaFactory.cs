using System.IO;
using Orchard.ContentManagement;

namespace Orchard.Media
{
    public interface IMediaFactory
    {
        IContent CreateMedia(Stream stream, string path, string mimeType, long length, string contentType);
    }
}