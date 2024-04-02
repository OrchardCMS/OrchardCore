using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public interface IFileBuilder
    {
        Task SetFileAsync(string subpath, Stream stream);
    }

    public static class IFileBuilderExtensions
    {
        public static async Task SetFileAsync(this IFileBuilder fileBuilder, string subpath, byte[] content)
        {
            using var stream = new MemoryStream(content);
            await fileBuilder.SetFileAsync(subpath, stream);
        }
    }
}
