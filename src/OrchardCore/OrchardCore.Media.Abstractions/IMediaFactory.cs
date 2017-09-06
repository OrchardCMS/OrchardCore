using System.IO;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media
{
    public interface IMediaFactory
    {
        IContent CreateMedia(Stream stream, string path, string mimeType, long length, string contentType);
    }
}