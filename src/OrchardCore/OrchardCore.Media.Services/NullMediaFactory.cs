using System.IO;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media
{
    public class NullMediaFactory : IMediaFactory
    {
        public static IMediaFactory Instance => new NullMediaFactory();

        public Task<IContent> CreateMediaAsync(Stream stream, string path, string mimeType, long length, string contentType)
        {
            return Task.FromResult<IContent>(null);
        }
    }
}
