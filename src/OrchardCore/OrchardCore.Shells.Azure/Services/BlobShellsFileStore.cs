using OrchardCore.FileStorage;

namespace OrchardCore.Shells.Azure.Services;

public class BlobShellsFileStore : IShellsFileStore
{
    private readonly IFileStore _fileStore;

    public BlobShellsFileStore(IFileStore fileStore)
    {
        _fileStore = fileStore;
    }

    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream)
        => _fileStore.CreateFileFromStreamAsync(path, inputStream, true);

    public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        => _fileStore.GetFileInfoAsync(path);

    public Task<Stream> GetFileStreamAsync(string path)
        => _fileStore.GetFileStreamAsync(path);

    public Task RemoveFileAsync(string path)
        => _fileStore.TryDeleteFileAsync(path);
}
