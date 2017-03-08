using System.Threading.Tasks;

namespace Orchard.Deployment
{
    public interface IFileBuilder
    {
        Task SetFileAsync(string subpath, byte[] content);
    }
}
