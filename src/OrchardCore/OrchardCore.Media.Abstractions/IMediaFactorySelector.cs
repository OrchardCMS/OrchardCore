using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Media
{
    public interface IMediaFactorySelector
    {
        Task<MediaFactorySelectorResult> GetMediaFactoryAsync(Stream stream, string fileName, string mimeType, string contentType);
    }
}
