using OrchardCore.FileStorage;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Shells.Azure.Services
{
    //TODO abstractions project, or possibly OrchardCore.Abstractions
    public interface IShellsFileStore
    {
        Task<IFileStoreEntry> GetFileInfoAsync(string path);
        Task<Stream> GetFileStreamAsync(string path);
        Task<string> CreateFileFromStreamAsync(string path, Stream inputStream);
    }
}
