using System.Threading.Tasks;

namespace OrchardCore.FileStorage
{
    public interface IFileStoreVersionProvider
    {
        Task<string> AddFileVersionToPathAsync(string resolvedPath, string path);
    }
}
