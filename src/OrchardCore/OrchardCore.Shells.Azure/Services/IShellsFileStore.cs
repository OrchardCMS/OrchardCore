using System.IO;
using System.Threading.Tasks;
using OrchardCore.FileStorage;

namespace OrchardCore.Shells.Azure.Services
{
    public interface IShellsFileStore
    {
        Task<IFileStoreEntry> GetFileInfoAsync(string path);
        Task<Stream> GetFileStreamAsync(string path);
        Task<string> CreateFileFromStreamAsync(string path, Stream inputStream);
        Task RemoveFileAsync(string path);
    }
}
