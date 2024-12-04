using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.Shells.Azure.Services;

public class BlobShellsFileStore : IShellsFileStore
{
    private readonly IFileStore _fileStore;
    private readonly MediaOptions _mediaOptions;

    public BlobShellsFileStore(IFileStore fileStore, MediaOptions mediaOptions)
    {
        _fileStore = fileStore;
        _mediaOptions = mediaOptions;
    }

    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream)
        => _fileStore.CreateFileFromStreamAsync(path, inputStream, _mediaOptions.OverwriteMedia);

    public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        => _fileStore.GetFileInfoAsync(path);

    public Task<Stream> GetFileStreamAsync(string path)
        => _fileStore.GetFileStreamAsync(path);

    public Task RemoveFileAsync(string path)
        => _fileStore.TryDeleteFileAsync(path);
}
