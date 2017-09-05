using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public interface IFileBuilder
    {
        Task SetFileAsync(string subpath, byte[] content);
    }
}
