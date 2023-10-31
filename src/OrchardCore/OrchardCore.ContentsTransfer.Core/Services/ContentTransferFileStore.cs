using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OrchardCore.ContentTransfer;
using OrchardCore.FileStorage;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentTransferFileStore : IContentTransferFileStore
{
    private readonly IFileStore _fileStore;

    public ContentTransferFileStore(IFileStore fileStore)
    {
        _fileStore = fileStore;
    }
    public Task CopyFileAsync(string srcPath, string dstPath)
        => _fileStore.CopyFileAsync(srcPath, dstPath);
    
    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        => _fileStore.CreateFileFromStreamAsync(path, inputStream, overwrite);

    public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        => _fileStore.GetDirectoryContentAsync(path, includeSubDirectories);

    public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        => _fileStore.GetDirectoryInfoAsync(path);

    public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        => _fileStore.GetFileInfoAsync(path);

    public Task<Stream> GetFileStreamAsync(string path)
        => _fileStore.GetFileStreamAsync(path);

    public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        => _fileStore.GetFileStreamAsync(fileStoreEntry);

    public Task MoveFileAsync(string oldPath, string newPath)
        => _fileStore.MoveFileAsync(oldPath, newPath);

    public Task<bool> TryCreateDirectoryAsync(string path)
        => _fileStore.TryCreateDirectoryAsync(path);

    public Task<bool> TryDeleteDirectoryAsync(string path)
        => _fileStore.TryDeleteDirectoryAsync(path);

    public Task<bool> TryDeleteFileAsync(string path)
        => _fileStore.TryDeleteFileAsync(path);
}
