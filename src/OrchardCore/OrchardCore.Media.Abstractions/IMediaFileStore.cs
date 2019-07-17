using System.Threading.Tasks;
using OrchardCore.FileStorage;

namespace OrchardCore.Media
{
    /// <summary>
    /// Represents an abstraction over a specialized file store for storing media and service it to clients.
    /// </summary>
    public interface IMediaFileStore : IFileStore, IMediaFileStorePathProvider
    {
    }
}
