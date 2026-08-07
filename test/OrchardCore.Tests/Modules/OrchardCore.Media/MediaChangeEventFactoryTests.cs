using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Realtime;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

/// <summary>
/// The point of carrying the affected entry in the broadcast is that the server resolves it once, instead
/// of every connected client reloading the directory. These tests pin that down.
/// </summary>
public class MediaChangeEventFactoryTests
{
    [Fact]
    public async Task IncludesTheEntry_UsingASingleFileStoreLookup()
    {
        var store = new SingleFileStore("photos/a.jpg");
        var factory = CreateFactory(store, withHttpContext: true);

        var result = await factory.CreateAsync("fileUploaded", "photos/a.jpg", includeItem: true);

        Assert.Equal("fileUploaded", result.Action);
        Assert.NotNull(result.Item);
        Assert.Equal("a.jpg", result.Item.Name);
        Assert.Equal("photos", result.Item.DirectoryPath);
        Assert.False(result.Item.IsDirectory);

        // One lookup per event, regardless of how many clients are connected. That is the whole point:
        // it replaces one directory listing per client.
        Assert.Equal(1, store.GetFileInfoCalls);
    }

    [Fact]
    public async Task UsesTheDestination_ForMovesAndCopies()
    {
        var store = new SingleFileStore("photos/renamed.jpg");
        var factory = CreateFactory(store, withHttpContext: true);

        var result = await factory.CreateAsync("fileMoved", "photos/a.jpg", "photos/renamed.jpg", includeItem: true);

        Assert.Equal("photos/a.jpg", result.Path);
        Assert.Equal("photos/renamed.jpg", result.NewPath);
        Assert.NotNull(result.Item);
        Assert.Equal("renamed.jpg", result.Item.Name);
    }

    [Fact]
    public async Task OmitsTheEntry_WhenNotRequested()
    {
        var store = new SingleFileStore("photos/a.jpg");
        var factory = CreateFactory(store, withHttpContext: true);

        var result = await factory.CreateAsync("fileDeleted", "photos/a.jpg", includeItem: false);

        Assert.Null(result.Item);
        Assert.Equal(0, store.GetFileInfoCalls);
    }

    [Fact]
    public async Task OmitsTheEntry_OutsideOfARequest()
    {
        // The public URL is built from the request PathBase, which carries the tenant prefix. Guessing it
        // would produce broken URLs on prefixed tenants, so the entry is omitted and clients reload.
        var store = new SingleFileStore("photos/a.jpg");
        var factory = CreateFactory(store, withHttpContext: false);

        var result = await factory.CreateAsync("fileUploaded", "photos/a.jpg", includeItem: true);

        Assert.Null(result.Item);
    }

    [Fact]
    public async Task OmitsTheEntry_WhenTheFileIsGone()
    {
        var store = new SingleFileStore(existingPath: null);
        var factory = CreateFactory(store, withHttpContext: true);

        var result = await factory.CreateAsync("fileUploaded", "photos/missing.jpg", includeItem: true);

        Assert.Null(result.Item);
    }

    private static MediaChangeEventFactory CreateFactory(SingleFileStore store, bool withHttpContext)
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = withHttpContext ? new DefaultHttpContext() : null,
        };

        return new MediaChangeEventFactory(
            accessor,
            new FileExtensionContentTypeProvider(),
            new PassThroughFileVersionProvider(),
            store);
    }

    private sealed class PassThroughFileVersionProvider : IFileVersionProvider
    {
        public string AddFileVersionToPath(PathString requestPathBase, string path) => path;
    }

    private sealed class SingleFileStore : IMediaFileStore
    {
        private readonly string _existingPath;

        public SingleFileStore(string existingPath) => _existingPath = existingPath;

        public int GetFileInfoCalls { get; private set; }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            GetFileInfoCalls++;

            return Task.FromResult<IFileStoreEntry>(
                path == _existingPath ? new Entry(path) : null);
        }

        public string MapPathToPublicUrl(string path) => "/media/" + path;

        public IFileStoreCapabilities Capabilities => FileStoreCapabilities.Default;

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path) => Task.FromResult<IFileStoreEntry>(null);

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            await Task.CompletedTask;

            yield break;
        }

        public Task<bool> TryCreateDirectoryAsync(string path) => Task.FromResult(true);

        public Task<bool> TryDeleteFileAsync(string path) => Task.FromResult(true);

        public Task<bool> TryDeleteDirectoryAsync(string path) => Task.FromResult(true);

        public Task MoveFileAsync(string oldPath, string newPath) => Task.CompletedTask;

        public Task CopyFileAsync(string srcPath, string dstPath) => Task.CompletedTask;

        public Task<Stream> GetFileStreamAsync(string path) => Task.FromResult<Stream>(new MemoryStream());

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry) => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
            => Task.FromResult(path);

        private sealed class Entry : IFileStoreEntry
        {
            public Entry(string path)
            {
                Path = path;
                var index = path.LastIndexOf('/');
                Name = index >= 0 ? path[(index + 1)..] : path;
                DirectoryPath = index >= 0 ? path[..index] : string.Empty;
            }

            public string Path { get; }

            public string Name { get; }

            public string DirectoryPath { get; }

            public long Length => 123;

            public DateTime LastModifiedUtc => DateTime.UtcNow;

            public bool IsDirectory => false;
        }
    }
}
