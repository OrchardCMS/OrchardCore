using Azure.Storage.Blobs;
using Microsoft.AspNetCore.StaticFiles;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

/// <summary>
/// Integration tests for <see cref="BlobFileStore"/> that run against Azurite.
/// Subclasses set <see cref="IsHnsEnabled"/> to exercise Gen1 (flat) or Gen2 (HNS) code paths.
/// </summary>
public abstract class BlobFileStoreTestsBase : IAsyncLifetime
{
    private const string EnvVar = "AZURITE_CONNECTION_STRING";

    protected abstract bool IsHnsEnabled { get; }

    private BlobFileStore _store;
    private BlobContainerClient _containerClient;
    private string _containerName;

    protected static string GetConnectionString()
        => System.Environment.GetEnvironmentVariable(EnvVar);

    public async ValueTask InitializeAsync()
    {
        var connectionString = GetConnectionString();
        _containerName = $"test-{Guid.NewGuid():N}";

        var options = new TestBlobStorageOptions
        {
            ConnectionString = connectionString,
            ContainerName = _containerName,
            BasePath = "",
            UseHierarchicalNamespace = IsHnsEnabled,
        };

        _containerClient = new BlobContainerClient(connectionString, _containerName);
        await _containerClient.CreateIfNotExistsAsync();

        var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);
        var contentTypeProvider = new FileExtensionContentTypeProvider();

        _store = new BlobFileStore(options, clock, contentTypeProvider);
        await _store.EnsureCapabilitiesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_containerClient is not null)
        {
            await _containerClient.DeleteIfExistsAsync();
        }
    }

    private async Task<string> CreateTestFileAsync(string path, string content = "test content")
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        return await _store.CreateFileFromStreamAsync(path, stream);
    }

    private async Task<string> ReadFileContentAsync(string path)
    {
        using var stream = await _store.GetFileStreamAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    // -- Capabilities --

    [AzuriteFact]
    public void EnsureCapabilities_SetsCorrectProvider()
    {
        var expected = IsHnsEnabled ? "Azure Blob (Gen2)" : "Azure Blob (Gen1)";
        Assert.Equal(expected, _store.Capabilities.StorageProvider);
    }

    [AzuriteFact]
    public void EnsureCapabilities_SetsHnsFlag()
    {
        Assert.Equal(IsHnsEnabled, _store.Capabilities.HasHierarchicalNamespace);
    }

    [AzuriteFact]
    public void EnsureCapabilities_SetsAtomicMoveFlag()
    {
        Assert.Equal(IsHnsEnabled, _store.Capabilities.SupportsAtomicMove);
    }

    // -- File operations --

    [AzuriteFact]
    public async Task CreateFile_ReturnsPath()
    {
        var result = await CreateTestFileAsync("folder/file.txt");
        Assert.Equal("folder/file.txt", result);
    }

    [AzuriteFact]
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

    [AzuriteFact]
    public async Task GetFileInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetFileInfoAsync("does-not-exist.txt");
        Assert.Null(info);
    }

    [AzuriteFact]
    public async Task GetFileStream_ReturnsContent()
    {
        var expected = "stream content test";
        await CreateTestFileAsync("stream-test.txt", expected);

        var actual = await ReadFileContentAsync("stream-test.txt");

        Assert.Equal(expected, actual);
    }

    [AzuriteFact]
    public async Task GetFileStream_NonExistent_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.GetFileStreamAsync("no-such-file.txt"));
    }

    [AzuriteFact]
    public async Task DeleteFile_ReturnsTrue()
    {
        await CreateTestFileAsync("delete-me.txt");

        var result = await _store.TryDeleteFileAsync("delete-me.txt");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("delete-me.txt"));
    }

    [AzuriteFact]
    public async Task DeleteFile_NonExistent_ReturnsFalse()
    {
        var result = await _store.TryDeleteFileAsync("ghost.txt");
        Assert.False(result);
    }

    [AzuriteFact]
    public async Task CopyFile_CreatesNewFile()
    {
        var content = "copy me";
        await CreateTestFileAsync("original.txt", content);

        await _store.CopyFileAsync("original.txt", "copied.txt");

        Assert.Equal(content, await ReadFileContentAsync("original.txt"));
        Assert.Equal(content, await ReadFileContentAsync("copied.txt"));
    }

    [AzuriteFact]
    public async Task CopyFile_SamePath_Throws()
    {
        await CreateTestFileAsync("same.txt");

        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CopyFileAsync("same.txt", "same.txt"));
    }

    [AzuriteFact]
    public async Task CreateFile_OverwriteTrue_Succeeds()
    {
        await CreateTestFileAsync("overwrite.txt", "v1");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await _store.CreateFileFromStreamAsync("overwrite.txt", stream, overwrite: true);

        var content = await ReadFileContentAsync("overwrite.txt");
        Assert.Equal("v2", content);
    }

    [AzuriteFact]
    public async Task CreateFile_OverwriteFalse_Throws()
    {
        await CreateTestFileAsync("no-overwrite.txt");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CreateFileFromStreamAsync("no-overwrite.txt", stream, overwrite: false));
    }

    // -- Directory operations --

    [AzuriteFact]
    public async Task GetDirectoryInfo_Root_ReturnsEntry()
    {
        var info = await _store.GetDirectoryInfoAsync(string.Empty);

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [AzuriteFact]
    public async Task GetDirectoryInfo_Existing_ReturnsEntry()
    {
        await _store.TryCreateDirectoryAsync("my-folder");

        var info = await _store.GetDirectoryInfoAsync("my-folder");

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
        Assert.Equal("my-folder", info.Path);
    }

    [AzuriteFact]
    public async Task GetDirectoryInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetDirectoryInfoAsync("no-such-folder");

        // Gen1 flat namespace: a directory "exists" if any blobs match the prefix.
        // Since there are none, it returns null.
        // Gen2 HNS: DataLake directory doesn't exist, returns null.
        Assert.Null(info);
    }

    [AzuriteFact]
    public async Task CreateDirectory_NewDirectory_Succeeds()
    {
        var result = await _store.TryCreateDirectoryAsync("new-dir");

        Assert.True(result);

        var info = await _store.GetDirectoryInfoAsync("new-dir");
        Assert.NotNull(info);
    }

    [AzuriteFact]
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

    [AzuriteFact]
    public async Task DeleteDirectory_NonExistent_ReturnsFalse()
    {
        var result = await _store.TryDeleteDirectoryAsync("phantom-dir");
        Assert.False(result);
    }

    [AzuriteFact]
    public async Task DeleteDirectory_Root_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.TryDeleteDirectoryAsync(string.Empty));
    }

    // -- Move --

    [AzuriteFact]
    public async Task MoveFile_MovesToNewPath()
    {
        var content = "move me";
        await CreateTestFileAsync("src.txt", content);

        await _store.MoveFileAsync("src.txt", "dst.txt");

        Assert.Null(await _store.GetFileInfoAsync("src.txt"));
        Assert.Equal(content, await ReadFileContentAsync("dst.txt"));
    }

    [AzuriteFact]
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

    [AzuriteFact]
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

    [AzuriteFact]
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

    [AzuriteFact]
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
}

/// <summary>
/// Concrete <see cref="BlobStorageOptions"/> for testing (the base class is abstract).
/// </summary>
internal sealed class TestBlobStorageOptions : BlobStorageOptions;

/// <summary>
/// Skips the test when the Azurite connection string is not configured.
/// Set the <c>AZURITE_CONNECTION_STRING</c> environment variable to run these tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class AzuriteFactAttribute : FactAttribute
{
    public AzuriteFactAttribute(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null,
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("AZURITE_CONNECTION_STRING")))
        {
            Skip = "Azurite is not configured. Set AZURITE_CONNECTION_STRING to run this test.";
        }
    }
}
