using Azure.Storage.Blobs;
using Microsoft.AspNetCore.StaticFiles;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;
using Testcontainers.Azurite;
using Xunit;

namespace OrchardCore.Tests.Integration.AzureBlob;

/// <summary>
/// Integration tests for <see cref="BlobFileStore"/> that run against Azurite.
/// Subclasses set <see cref="IsHnsEnabled"/> to exercise Gen1 (flat) or Gen2 (HNS) code paths.
/// </summary>
public abstract class BlobFileStoreTestsBase : IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.30.0")
        .WithCommand("--skipApiVersionCheck")
        .Build();

    protected abstract bool IsHnsEnabled { get; }

    private BlobFileStore _store;
    private BlobContainerClient _containerClient;
    private string _containerName;

    public async ValueTask InitializeAsync()
    {
        await _azuriteContainer.StartAsync()
            .ConfigureAwait(false);

        _containerName = $"test-{Guid.NewGuid():N}";

        _containerClient = new BlobContainerClient(_azuriteContainer.GetConnectionString(), _containerName);

        await _containerClient.CreateIfNotExistsAsync();

        var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);
        var contentTypeProvider = new FileExtensionContentTypeProvider();

        var blobStorageOptions = new TestBlobStorageOptions
        {
            ConnectionString = _azuriteContainer.GetConnectionString(),
            ContainerName = _containerName,
            BasePath = "",
            UseHierarchicalNamespace = IsHnsEnabled,
        };

        _store = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);

        await _store.EnsureCapabilitiesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_containerClient is not null)
        {
            await _containerClient.DeleteIfExistsAsync();
        }
    }

    protected IFileStoreCapabilities Capabilities => _store.Capabilities;

    protected async Task<string> CreateTestFileAsync(string path, string content = "test content")
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        return await _store.CreateFileFromStreamAsync(path, stream);
    }

    protected async Task<string> ReadFileContentAsync(string path)
    {
        using var stream = await _store.GetFileStreamAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    protected Task<bool> TryCreateDirectoryAsync(string path)
        => _store.TryCreateDirectoryAsync(path);

    protected Task<bool> TryDeleteDirectoryAsync(string path)
        => _store.TryDeleteDirectoryAsync(path);

    protected Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        => _store.GetDirectoryInfoAsync(path);

    protected Task<IFileStoreEntry> GetFileInfoAsync(string path)
        => _store.GetFileInfoAsync(path);

    protected IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        => _store.GetDirectoryContentAsync(path, includeSubDirectories);

    protected Task MoveFileAsync(string oldPath, string newPath)
        => _store.MoveFileAsync(oldPath, newPath);

    // -- Capabilities --

    [Fact]
    public void EnsureCapabilities_SetsHnsFlag()
    {
        Assert.Equal(IsHnsEnabled, _store.Capabilities.HasHierarchicalNamespace);
    }

    [Fact]
    public void EnsureCapabilities_SetsAtomicMoveFlag()
    {
        Assert.Equal(IsHnsEnabled, _store.Capabilities.SupportsAtomicMove);
    }

    // -- File operations --

    [Fact]
    public async Task CreateFile_ReturnsPath()
    {
        var result = await CreateTestFileAsync("folder/file.txt");
        Assert.Equal("folder/file.txt", result);
    }

    [Fact]
    public async Task GetFileInfo_ReturnsCorrectMetadata()
    {
        var content = "hello world";
        await CreateTestFileAsync("info-test.txt", content);

        var info = await _store.GetFileInfoAsync("info-test.txt");

        Assert.NotNull(info);
        Assert.Equal("info-test.txt", info.Path);
        Assert.Equal("info-test.txt", info.Name);
        Assert.Equal(content.Length, info.Length);
        Assert.False(info.IsDirectory);
    }

    [Fact]
    public async Task GetFileInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetFileInfoAsync("does-not-exist.txt");
        Assert.Null(info);
    }

    [Fact]
    public async Task GetFileStream_ReturnsContent()
    {
        var expected = "stream content test";
        await CreateTestFileAsync("stream-test.txt", expected);

        var actual = await ReadFileContentAsync("stream-test.txt");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetFileStream_NonExistent_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.GetFileStreamAsync("no-such-file.txt"));
    }

    [Fact]
    public async Task DeleteFile_ReturnsTrue()
    {
        await CreateTestFileAsync("delete-me.txt");

        var result = await _store.TryDeleteFileAsync("delete-me.txt");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("delete-me.txt"));
    }

    [Fact]
    public async Task DeleteFile_NonExistent_ReturnsFalse()
    {
        var result = await _store.TryDeleteFileAsync("ghost.txt");
        Assert.False(result);
    }

    [Fact]
    public async Task CopyFile_CreatesNewFile()
    {
        var content = "copy me";
        await CreateTestFileAsync("original.txt", content);

        await _store.CopyFileAsync("original.txt", "copied.txt");

        Assert.Equal(content, await ReadFileContentAsync("original.txt"));
        Assert.Equal(content, await ReadFileContentAsync("copied.txt"));
    }

    [Fact]
    public async Task CopyFile_SamePath_Throws()
    {
        await CreateTestFileAsync("same.txt");

        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CopyFileAsync("same.txt", "same.txt"));
    }

    [Fact]
    public async Task CreateFile_OverwriteTrue_Succeeds()
    {
        await CreateTestFileAsync("overwrite.txt", "v1");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await _store.CreateFileFromStreamAsync("overwrite.txt", stream, overwrite: true);

        var content = await ReadFileContentAsync("overwrite.txt");
        Assert.Equal("v2", content);
    }

    [Fact]
    public async Task CreateFile_OverwriteFalse_Throws()
    {
        await CreateTestFileAsync("no-overwrite.txt");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CreateFileFromStreamAsync("no-overwrite.txt", stream, overwrite: false));
    }

    // -- Directory operations --

    [Fact]
    public async Task GetDirectoryInfo_Root_ReturnsEntry()
    {
        var info = await _store.GetDirectoryInfoAsync(string.Empty);

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [Fact]
    public async Task GetDirectoryInfo_Existing_ReturnsEntry()
    {
        await _store.TryCreateDirectoryAsync("my-folder");

        var info = await _store.GetDirectoryInfoAsync("my-folder");

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
        Assert.Equal("my-folder", info.Path);
    }

    [Fact]
    public async Task GetDirectoryInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetDirectoryInfoAsync("no-such-folder");

        // Gen1 flat namespace: a directory "exists" if any blobs match the prefix.
        // Since there are none, it returns null.
        // Gen2 HNS: DataLake directory doesn't exist, returns null.
        Assert.Null(info);
    }

    [Fact]
    public async Task CreateDirectory_NewDirectory_Succeeds()
    {
        var result = await _store.TryCreateDirectoryAsync("new-dir");

        Assert.True(result);

        var info = await _store.GetDirectoryInfoAsync("new-dir");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task DeleteDirectory_WithContents_DeletesAll()
    {
        await _store.TryCreateDirectoryAsync("dir-to-delete");
        await CreateTestFileAsync("dir-to-delete/file1.txt");
        await CreateTestFileAsync("dir-to-delete/file2.txt");

        var result = await _store.TryDeleteDirectoryAsync("dir-to-delete");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("dir-to-delete/file1.txt"));
        Assert.Null(await _store.GetFileInfoAsync("dir-to-delete/file2.txt"));
    }

    [Fact]
    public async Task DeleteDirectory_NonExistent_ReturnsFalse()
    {
        var result = await _store.TryDeleteDirectoryAsync("phantom-dir");
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDirectory_Root_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.TryDeleteDirectoryAsync(string.Empty));
    }

    // -- Move --

    [Fact]
    public async Task MoveFile_MovesToNewPath()
    {
        var content = "move me";
        await CreateTestFileAsync("src.txt", content);

        await _store.MoveFileAsync("src.txt", "dst.txt");

        Assert.Null(await _store.GetFileInfoAsync("src.txt"));
        Assert.Equal(content, await ReadFileContentAsync("dst.txt"));
    }

    [Fact]
    public async Task MoveFile_AcrossDirectories()
    {
        await _store.TryCreateDirectoryAsync("dir-a");
        await _store.TryCreateDirectoryAsync("dir-b");
        await CreateTestFileAsync("dir-a/moved.txt", "data");

        await _store.MoveFileAsync("dir-a/moved.txt", "dir-b/moved.txt");

        Assert.Null(await _store.GetFileInfoAsync("dir-a/moved.txt"));
        Assert.Equal("data", await ReadFileContentAsync("dir-b/moved.txt"));
    }

    // -- Directory content listing --

    [Fact]
    public async Task GetDirectoryContent_ListsFilesAndDirs()
    {
        await CreateTestFileAsync("root-file.txt");
        await _store.TryCreateDirectoryAsync("sub-dir");
        await CreateTestFileAsync("sub-dir/nested.txt");

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in _store.GetDirectoryContentAsync())
        {
            entries.Add(entry);
        }

        Assert.Contains(entries, e => e.Name == "root-file.txt" && !e.IsDirectory);
        Assert.Contains(entries, e => e.Name == "sub-dir" && e.IsDirectory);
    }

    [Fact]
    public async Task GetDirectoryContent_ExcludesMarkerFiles()
    {
        await _store.TryCreateDirectoryAsync("marker-test");

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in _store.GetDirectoryContentAsync("marker-test"))
        {
            entries.Add(entry);
        }

        // The marker file (OrchardCore.Media.txt) used in Gen1 should never appear in listings.
        Assert.DoesNotContain(entries, e => e.Name == "OrchardCore.Media.txt");
    }

    [Fact]
    public async Task GetDirectoryContent_Flat_ListsNestedContent()
    {
        await CreateTestFileAsync("flat/a.txt");
        await CreateTestFileAsync("flat/sub/b.txt");

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in _store.GetDirectoryContentAsync("flat", includeSubDirectories: true))
        {
            entries.Add(entry);
        }

        Assert.Contains(entries, e => !e.IsDirectory && e.Name == "a.txt");
        Assert.Contains(entries, e => !e.IsDirectory && e.Name == "b.txt");
    }

    [Fact]
    public async Task CreateDirectory_AlreadyExists_ReturnsFalse()
    {
        await _store.TryCreateDirectoryAsync("existing-dir");

        // Creating the same directory again should indicate it already existed.
        var result = await _store.TryCreateDirectoryAsync("existing-dir");

        // Gen1 always returns true (no real directory to check).
        // Gen2 returns false because the directory already exists.
        if (IsHnsEnabled)
        {
            Assert.False(result);
        }
        else
        {
            Assert.True(result);
        }
    }

    [Fact]
    public async Task MoveFile_NonExistent_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.MoveFileAsync("no-such-file.txt", "destination.txt"));
    }

    [Fact]
    public async Task DeleteDirectory_WithNestedSubdirectories_DeletesAll()
    {
        await _store.TryCreateDirectoryAsync("parent");
        await _store.TryCreateDirectoryAsync("parent/child");
        await CreateTestFileAsync("parent/top.txt");
        await CreateTestFileAsync("parent/child/deep.txt");

        var result = await _store.TryDeleteDirectoryAsync("parent");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("parent/top.txt"));
        Assert.Null(await _store.GetFileInfoAsync("parent/child/deep.txt"));
        Assert.Null(await _store.GetDirectoryInfoAsync("parent"));
    }

    [Fact]
    public async Task GetDirectoryContent_Subdirectory_ListsOnlyDirectChildren()
    {
        await _store.TryCreateDirectoryAsync("listing");
        await CreateTestFileAsync("listing/file-a.txt");
        await _store.TryCreateDirectoryAsync("listing/inner");
        await CreateTestFileAsync("listing/inner/file-b.txt");

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in _store.GetDirectoryContentAsync("listing"))
        {
            entries.Add(entry);
        }

        Assert.Contains(entries, e => e.Name == "file-a.txt" && !e.IsDirectory);
        Assert.Contains(entries, e => e.Name == "inner" && e.IsDirectory);
        // file-b.txt is nested inside "inner", should not appear at this level.
        Assert.DoesNotContain(entries, e => e.Name == "file-b.txt");
    }

    [Fact]
    public async Task MoveFile_PreservesContent()
    {
        var content = "preserve this content across move";
        await CreateTestFileAsync("move-preserve.txt", content);

        await _store.MoveFileAsync("move-preserve.txt", "moved-preserve.txt");

        var actual = await ReadFileContentAsync("moved-preserve.txt");
        Assert.Equal(content, actual);
    }
}
