using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.AmazonS3;

/// <summary>
/// Integration tests for <see cref="AwsFileStore"/> that run against LocalStack's S3 emulator.
/// Set the <c>LOCALSTACK_CONNECTION_STRING</c> environment variable (e.g. <c>http://127.0.0.1:4566</c>) to run these tests.
/// </summary>
public sealed class AwsFileStoreTests : IAsyncLifetime
{
    private const string EnvVar = "LOCALSTACK_CONNECTION_STRING";

    private AwsFileStore _store;
    private AmazonS3Client _s3Client;
    private string _bucketName;

    private static string GetServiceUrl()
        => System.Environment.GetEnvironmentVariable(EnvVar);

    public async ValueTask InitializeAsync()
    {
        var serviceUrl = GetServiceUrl();
        _bucketName = $"test-{Guid.NewGuid():N}";

        _s3Client = new AmazonS3Client(
            new BasicAWSCredentials("test", "test"),
            new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
                // AWSSDK v4 enables checksums by default which LocalStack may not fully support.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            });

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
            catch
            {
                // Best effort cleanup.
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

    [LocalStackFact]
    public async Task CreateFile_ReturnsPath()
    {
        var result = await CreateTestFileAsync("folder/file.txt");
        Assert.Equal("folder/file.txt", result);
    }

    [LocalStackFact]
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

    [LocalStackFact]
    public async Task GetFileInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetFileInfoAsync("does-not-exist.txt");
        Assert.Null(info);
    }

    [LocalStackFact]
    public async Task GetFileStream_ReturnsContent()
    {
        var expected = "stream content test";
        await CreateTestFileAsync("stream-test.txt", expected);

        var actual = await ReadFileContentAsync("stream-test.txt");

        Assert.Equal(expected, actual);
    }

    [LocalStackFact]
    public async Task DeleteFile_ReturnsTrue()
    {
        await CreateTestFileAsync("delete-me.txt");

        var result = await _store.TryDeleteFileAsync("delete-me.txt");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("delete-me.txt"));
    }

    [LocalStackFact]
    public async Task CopyFile_CreatesNewFile()
    {
        var content = "copy me";
        await CreateTestFileAsync("original.txt", content);

        await _store.CopyFileAsync("original.txt", "copied.txt");

        Assert.Equal(content, await ReadFileContentAsync("original.txt"));
        Assert.Equal(content, await ReadFileContentAsync("copied.txt"));
    }

    [LocalStackFact]
    public async Task CopyFile_SamePath_Throws()
    {
        await CreateTestFileAsync("same.txt");

        await Assert.ThrowsAnyAsync<Exception>(
            () => _store.CopyFileAsync("same.txt", "same.txt"));
    }

    [LocalStackFact]
    public async Task CopyFile_SourceNotFound_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CopyFileAsync("ghost.txt", "dest.txt"));
    }

    [LocalStackFact]
    public async Task CreateFile_OverwriteTrue_Succeeds()
    {
        await CreateTestFileAsync("overwrite.txt", "v1");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await _store.CreateFileFromStreamAsync("overwrite.txt", stream, overwrite: true);

        var content = await ReadFileContentAsync("overwrite.txt");
        Assert.Equal("v2", content);
    }

    [LocalStackFact]
    public async Task CreateFile_OverwriteFalse_Throws()
    {
        await CreateTestFileAsync("no-overwrite.txt");

        using var stream = new MemoryStream("v2"u8.ToArray());
        await Assert.ThrowsAnyAsync<FileStoreException>(
            () => _store.CreateFileFromStreamAsync("no-overwrite.txt", stream, overwrite: false));
    }

    // -- Directory operations --

    [LocalStackFact]
    public async Task GetDirectoryInfo_Root_ReturnsEntry()
    {
        var info = await _store.GetDirectoryInfoAsync(string.Empty);

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [LocalStackFact]
    public async Task GetDirectoryInfo_Existing_ReturnsEntry()
    {
        await _store.TryCreateDirectoryAsync("my-folder");

        var info = await _store.GetDirectoryInfoAsync("my-folder");

        Assert.NotNull(info);
        Assert.True(info.IsDirectory);
    }

    [LocalStackFact]
    public async Task GetDirectoryInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetDirectoryInfoAsync("no-such-folder");
        Assert.Null(info);
    }

    [LocalStackFact]
    public async Task CreateDirectory_Succeeds()
    {
        var result = await _store.TryCreateDirectoryAsync("new-dir");

        Assert.True(result);

        var info = await _store.GetDirectoryInfoAsync("new-dir");
        Assert.NotNull(info);
    }

    [LocalStackFact]
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

    [LocalStackFact]
    public async Task DeleteDirectory_Root_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.TryDeleteDirectoryAsync(string.Empty));
    }

    // -- Move --

    [LocalStackFact]
    public async Task MoveFile_MovesToNewPath()
    {
        var content = "move me";
        await CreateTestFileAsync("src.txt", content);

        await _store.MoveFileAsync("src.txt", "dst.txt");

        Assert.Null(await _store.GetFileInfoAsync("src.txt"));
        Assert.Equal(content, await ReadFileContentAsync("dst.txt"));
    }

    [LocalStackFact]
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

    [LocalStackFact]
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
}

/// <summary>
/// Skips the test when the LocalStack connection string is not configured.
/// Set the <c>LOCALSTACK_CONNECTION_STRING</c> environment variable to run these tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class LocalStackFactAttribute : FactAttribute
{
    public LocalStackFactAttribute(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null,
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("LOCALSTACK_CONNECTION_STRING")))
        {
            Skip = "LocalStack is not configured. Set LOCALSTACK_CONNECTION_STRING to run this test.";
        }
    }
}
