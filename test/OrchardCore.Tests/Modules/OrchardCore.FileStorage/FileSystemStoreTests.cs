using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;

namespace OrchardCore.Tests.Modules.OrchardCore.FileStorage;

/// <summary>
/// Tests for the local file system media storage, focused on folder names containing
/// characters that NTFS rejects (e.g. ':'), see https://github.com/OrchardCMS/OrchardCore/issues/17644.
/// Unlike remote stores, local storage is bound by the rules of the underlying file system:
/// such names work on Linux and fail with a <see cref="FileStoreException"/> on Windows.
/// </summary>
public class FileSystemStoreTests : IDisposable
{
    private const string NtfsInvalidFolder = "test:asdf";

    private readonly string _root;
    private readonly FileSystemStore _store;

    public FileSystemStoreTests()
    {
        _root = Directory.CreateTempSubdirectory("fs-store-tests").FullName;
        _store = new FileSystemStore(_root, NullLogger<FileSystemStore>.Instance);
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private async Task<string> CreateFileAsync(string path, string content = "test content")
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return await _store.CreateFileFromStreamAsync(path, stream);
    }

    private async Task<string> ReadFileContentAsync(string path)
    {
        using var stream = await _store.GetFileStreamAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task CreateFile_ValidFolder_RoundTrips()
    {
        var result = await CreateFileAsync("folder/file.txt", "hello");

        Assert.Equal("folder/file.txt", result);
        Assert.Equal("hello", await ReadFileContentAsync("folder/file.txt"));

        var info = await _store.GetFileInfoAsync("folder/file.txt");
        Assert.NotNull(info);
        Assert.Equal("file.txt", info.Name);
    }

    [Fact]
    public async Task CreateFile_FolderWithNtfsInvalidCharacters_DependsOnFileSystem()
    {
        var path = $"{NtfsInvalidFolder}/file.txt";

        if (OperatingSystem.IsWindows())
        {
            // NTFS rejects ':' in names; the store surfaces it as a FileStoreException.
            await Assert.ThrowsAsync<FileStoreException>(() => CreateFileAsync(path));
        }
        else
        {
            await CreateFileAsync(path, "ntfs invalid");

            var info = await _store.GetFileInfoAsync(path);
            Assert.NotNull(info);
            Assert.Equal(path, info.Path);
            Assert.Equal("ntfs invalid", await ReadFileContentAsync(path));

            Assert.True(await _store.TryDeleteDirectoryAsync(NtfsInvalidFolder));
            Assert.Null(await _store.GetFileInfoAsync(path));
        }
    }

    [Fact]
    public async Task CreateDirectory_FolderWithNtfsInvalidCharacters_DependsOnFileSystem()
    {
        if (OperatingSystem.IsWindows())
        {
            await Assert.ThrowsAsync<FileStoreException>(() => _store.TryCreateDirectoryAsync(NtfsInvalidFolder));
        }
        else
        {
            Assert.True(await _store.TryCreateDirectoryAsync(NtfsInvalidFolder));
            Assert.NotNull(await _store.GetDirectoryInfoAsync(NtfsInvalidFolder));

            var entries = new List<IFileStoreEntry>();
            await foreach (var entry in _store.GetDirectoryContentAsync())
            {
                entries.Add(entry);
            }

            Assert.Contains(entries, e => e.IsDirectory && e.Name == NtfsInvalidFolder);
        }
    }
}
