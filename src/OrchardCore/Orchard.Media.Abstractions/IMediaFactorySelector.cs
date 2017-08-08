using System.IO;

namespace Orchard.Media
{
    public interface IMediaFactorySelector
    {
        MediaFactorySelectorResult GetMediaFactory(Stream stream, string fileName, string mimeType, string contentType);
    }
}