using System.IO;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media
{
    public interface IMediaFactory
    {
        Task<IContent> CreateMediaAsync(Stream stream, string path, string mimeType, long length, string contentType);
    }
}