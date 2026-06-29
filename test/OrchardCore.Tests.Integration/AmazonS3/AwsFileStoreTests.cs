using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Modules;
using OrchardCore.Tests.Integration.Infrastructure;
using Xunit;

namespace OrchardCore.Tests.Integration.AmazonS3;

/// <summary>
/// Integration tests for <see cref="AwsFileStore"/> that run against a LocalStack S3 emulator
/// started automatically by Testcontainers. The tests are skipped when Docker is not available.
/// </summary>
[Collection(LocalStackCollection.Name)]
public sealed class AwsFileStoreTests : IAsyncLifetime
{
    private readonly LocalStackFixture _fixture;
    private readonly ITestOutputHelper _output;
    private AwsFileStore _store;
    private AmazonS3Client _s3Client;
    private string _bucketName;

    public AwsFileStoreTests(LocalStackFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public async ValueTask InitializeAsync()
    {
        if (!DockerSupport.IsAvailable)
        {
            return;
        }

        _bucketName = $"test-{Guid.NewGuid():N}";
        _s3Client = _fixture.CreateClient();

        await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName });

        var options = new AwsStorageOptions
        {
            BucketName = _bucketName,
            BasePath = "",
        };

        var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);
        _store = new AwsFileStore(clock, options, _s3Client);
    }

    public async ValueTask DisposeAsync()
    {
        if (_s3Client is not null && _bucketName is not null)
        {
            try
            {
                // Delete all objects first (S3 requires empty bucket before deletion).
                var listResponse = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                });

                if (listResponse.S3Objects.Count > 0)
                {
                    await _s3Client.DeleteObjectsAsync(new DeleteObjectsRequest
                    {
                        BucketName = _bucketName,
                        Objects = listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList(),
                    });
                }

                await _s3Client.DeleteBucketAsync(_bucketName);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Best effort cleanup failed: {ex.Message}");
            }

            _s3Client.Dispose();
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

    // -- File operations --

    [DockerFact]
    public async Task CreateFile_Default_ReturnsPath()
    {
        var result = await CreateTestFileAsync("folder/file.txt");
        Assert.Equal("folder/file.txt", result);
    }

    [DockerFact]
    public async Task GetFileInfo_Default_ReturnsCorrectMetadata()
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

    [DockerFact]
    public async Task GetFileInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetFileInfoAsync("does-not-exist.txt");
        Assert.Null(info);
    }

    [DockerFact]
    public async Task GetFileStream_Default_ReturnsContent()
    {
        var expected = "stream content test";
        await CreateTestFileAsync("stream-test.txt", expected);

        var actual = await ReadFileContentAsync("stream-test.txt");

        Assert.Equal(expected, actual);
    }

    [DockerFact]
    public async Task DeleteFile_Default_ReturnsTrue()
    {
        await CreateTestFileAsync("delete-me.txt");

        var result = await _store.TryDeleteFileAsync("delete-me.txt");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("delete-me.txt"));
    }

    [DockerFact]
    public async Task CopyFile_Default_CreatesNewFile()
    {
        var content = "copy me";
        await CreateTestFileAsync("original.txt", content);

        await _store.CopyFileAsync("original.txt", "copied.txt");

        Assert.Equal(content, await ReadFileContentAsync("original.txt"));
        Assert.Equal(content, await ReadFileContentAsync("copied.txt"));
    }

    [DockerFact]
    public async Task CopyFile_SamePath_Throws()
    {
        await CreateTestFileAsync("same.txt");

        await Assert.ThrowsAnyAsync<Exception>(
            () => _store.CopyFileAsync("same.txt", "same.txt"));
    }

    [DockerFact]
    public async Task CopyFile_SourceNotFound_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CopyFileAsync("ghost.txt", "dest.txt"));
    }

    [DockerFact]
    public async Task CreateFile_OverwriteTrue_Succeeds()
    {
        await CreateTestFileAsync("overwrite.txt", "v1");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await _store.CreateFileFromStreamAsync("overwrite.txt", stream, overwrite: true);

        var content = await ReadFileContentAsync("overwrite.txt");
        Assert.Equal("v2", content);
    }

    [DockerFact]
    public async Task CreateFile_OverwriteFalse_Throws()
    {
        await CreateTestFileAsync("no-overwrite.txt");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await Assert.ThrowsAnyAsync<FileStoreException>(
            () => _store.CreateFileFromStreamAsync("no-overwrite.txt", stream, overwrite: false));
    }

    // -- Directory operations --

    [DockerFact]
    public async Task GetDirectoryInfo_Root_ReturnsEntry()
    {
        var info = await _store.GetDirectoryInfoAsync(string.Empty);

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [DockerFact]
    public async Task GetDirectoryInfo_Existing_ReturnsEntry()
    {
        await _store.TryCreateDirectoryAsync("my-folder");

        var info = await _store.GetDirectoryInfoAsync("my-folder");

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [DockerFact]
    public async Task GetDirectoryInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetDirectoryInfoAsync("no-such-folder");
        Assert.Null(info);
    }

    [DockerFact]
    public async Task CreateDirectory_Default_Succeeds()
    {
        var result = await _store.TryCreateDirectoryAsync("new-dir");

        Assert.True(result);

        var info = await _store.GetDirectoryInfoAsync("new-dir");
        Assert.NotNull(info);
    }

    [DockerFact]
    public async Task DeleteDirectory_Contents_DeletesAll()
    {
        await _store.TryCreateDirectoryAsync("dir-to-delete");
        await CreateTestFileAsync("dir-to-delete/file1.txt");
        await CreateTestFileAsync("dir-to-delete/file2.txt");

        var result = await _store.TryDeleteDirectoryAsync("dir-to-delete");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("dir-to-delete/file1.txt"));
        Assert.Null(await _store.GetFileInfoAsync("dir-to-delete/file2.txt"));
    }

    [DockerFact]
    public async Task DeleteDirectory_Root_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.TryDeleteDirectoryAsync(string.Empty));
    }

    // -- Move --

    [DockerFact]
    public async Task MoveFile_Default_MovesToNewPath()
    {
        var content = "move me";
        await CreateTestFileAsync("src.txt", content);

        await _store.MoveFileAsync("src.txt", "dst.txt");

        Assert.Null(await _store.GetFileInfoAsync("src.txt"));
        Assert.Equal(content, await ReadFileContentAsync("dst.txt"));
    }

    [DockerFact]
    public async Task MoveFile_AcrossDirectories_Succeeds()
    {
        await _store.TryCreateDirectoryAsync("dir-a");
        await _store.TryCreateDirectoryAsync("dir-b");
        await CreateTestFileAsync("dir-a/moved.txt", "data");

        await _store.MoveFileAsync("dir-a/moved.txt", "dir-b/moved.txt");

        Assert.Null(await _store.GetFileInfoAsync("dir-a/moved.txt"));
        Assert.Equal("data", await ReadFileContentAsync("dir-b/moved.txt"));
    }

    // -- Directory content listing --

    [DockerFact]
    public async Task GetDirectoryContent_Default_ListsFilesAndDirs()
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
}

