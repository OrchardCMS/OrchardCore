using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Modules;
using Testcontainers.LocalStack;
using Xunit;

namespace OrchardCore.Tests.Integration.AmazonS3;

public class AwsFileStoreTests : IAsyncLifetime
{
    private AwsFileStore _store;
    private AmazonS3Client _s3Client;
    private string _bucketName;

    private readonly LocalStackContainer _localStackContainer = new LocalStackBuilder("localstack/localstack:2.0")
        .Build();

    static AwsFileStoreTests()
    {
        System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", CommonCredentials.AwsAccessKey);
        System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", CommonCredentials.AwsSecretKey);
    }

    public async ValueTask InitializeAsync()
    {
        await _localStackContainer.StartAsync()
            .ConfigureAwait(false);

        _bucketName = $"test-{Guid.NewGuid():N}";

        _s3Client = new AmazonS3Client(
            new AmazonS3Config
            {
                ServiceURL = _localStackContainer.GetConnectionString(),
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
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
    public async Task DeleteFile_ReturnsTrue()
    {
        await CreateTestFileAsync("delete-me.txt");

        var result = await _store.TryDeleteFileAsync("delete-me.txt");

        Assert.True(result);
        Assert.Null(await _store.GetFileInfoAsync("delete-me.txt"));
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

        await Assert.ThrowsAnyAsync<Exception>(
            () => _store.CopyFileAsync("same.txt", "same.txt"));
    }

    [Fact]
    public async Task CopyFile_SourceNotFound_Throws()
    {
        await Assert.ThrowsAsync<FileStoreException>(
            () => _store.CopyFileAsync("ghost.txt", "dest.txt"));
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
        await Assert.ThrowsAnyAsync<FileStoreException>(
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
    }

    [Fact]
    public async Task GetDirectoryInfo_NonExistent_ReturnsNull()
    {
        var info = await _store.GetDirectoryInfoAsync("no-such-folder");
        Assert.Null(info);
    }

    [Fact]
    public async Task CreateDirectory_Succeeds()
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

    public async ValueTask DisposeAsync()
    {
        await _localStackContainer.StopAsync();

        GC.SuppressFinalize(this);
    }

    //public sealed class LocalStackDefaultConfiguration : AwsFileStoreTests
    //{
    //    public LocalStackDefaultConfiguration() : base(new LocalStackBuilder(TestSession.GetImageFromDockerfile()).Build())
    //    {
    //    }
    //}
}
