using System.IO;

namespace OrchardCore.Media
{
    public interface IMediaFactorySelector
    {
        MediaFactorySelectorResult GetMediaFactory(Stream stream, string fileName, string mimeType, string contentType);
    }
}