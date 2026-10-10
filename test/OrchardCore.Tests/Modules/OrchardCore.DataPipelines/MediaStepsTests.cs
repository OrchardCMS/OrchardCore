using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class MediaStepsTests
{
    [Fact]
    public async Task SaveToMedia_File_SavesItInTheFolderAndRecordsItsUrl()
    {
        // Arrange
        var store = new InMemoryMediaStore();
        var settings = new SaveToMediaStepSettings { Folder = "exports/daily" };
        using var host = new StepTestHost(new SaveToMediaStep(new PassThroughStringLocalizer<SaveToMediaStep>()), settings, Services(store));
        host.WithFiles(host.CreateFile("orders.csv", "Id\r\n1\r\n"));

        // Act
        await host.ExecuteAsync();

        // Assert
        Assert.Equal("Id\r\n1\r\n", store.Read("exports/daily/orders.csv"));
        var delivery = Assert.Single(host.Run.Deliveries);
        Assert.Equal("/media/exports/daily/orders.csv", delivery.Url);
    }

    [Fact]
    public async Task SaveToMedia_ExistingFileWithoutOverwrite_SavesUnderANewName()
    {
        // Arrange
        var store = new InMemoryMediaStore();
        store.Write("orders.csv", "old");
        var settings = new SaveToMediaStepSettings { Overwrite = false };
        using var host = new StepTestHost(new SaveToMediaStep(new PassThroughStringLocalizer<SaveToMediaStep>()), settings, Services(store));
        host.WithFiles(host.CreateFile("orders.csv", "new"));

        // Act
        await host.ExecuteAsync();

        // Assert
        Assert.Equal("old", store.Read("orders.csv"));
        Assert.Equal("new", store.Read("orders (2).csv"));
    }

    [Fact]
    public async Task SaveToMedia_ExtensionNotAllowed_FailsWithoutSaving()
    {
        // Arrange
        var store = new InMemoryMediaStore();
        using var host = new StepTestHost(new SaveToMediaStep(new PassThroughStringLocalizer<SaveToMediaStep>()), new SaveToMediaStepSettings(), Services(store));
        host.WithFiles(host.CreateFile("page.html", "<script></script>"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => host.ExecuteAsync());
        Assert.Empty(store.Files);
    }

    [Theory]
    [InlineData("../secrets")]
    [InlineData("a/../../b")]
    public async Task SaveToMedia_FolderOutsideTheMediaLibrary_ReportsError(string folder)
    {
        // Arrange
        using var host = new StepTestHost(new SaveToMediaStep(new PassThroughStringLocalizer<SaveToMediaStep>()), new SaveToMediaStepSettings { Folder = folder }, Services(new InMemoryMediaStore()));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task SaveToMedia_UserWhoCannotManageTheFolder_Fails()
    {
        // Arrange
        var store = new InMemoryMediaStore();
        using var host = new StepTestHost(new SaveToMediaStep(new PassThroughStringLocalizer<SaveToMediaStep>()), new SaveToMediaStepSettings(), Services(store, authorized: false));
        host.WithFiles(host.CreateFile("orders.csv", "x"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => host.ExecuteAsync());
        Assert.Empty(store.Files);
    }

    [Fact]
    public async Task ReadMediaFile_Csv_ReadsRowsAndDescribesFields()
    {
        // Arrange
        var store = new InMemoryMediaStore();
        store.Write("imports/people.csv", "Id,Name\r\n1,Ada\r\n2,Bob\r\n");
        var settings = new ReadMediaFileStepSettings { Path = "imports/people.csv" };
        using var host = new StepTestHost(new ReadMediaFileStep(new PassThroughStringLocalizer<ReadMediaFileStep>()), settings, Services(store));

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync();

        // Assert
        Assert.Equal(["Id", "Name"], description.Outputs[DataPipelinePort.Output].Select(field => field.Name));
        Assert.Equal([["1", "Ada"], ["2", "Bob"]], host.Output().Rows);
    }

    [Fact]
    public async Task ReadMediaFile_MissingFile_ReportsError()
    {
        // Arrange
        var settings = new ReadMediaFileStepSettings { Path = "nope.csv" };
        using var host = new StepTestHost(new ReadMediaFileStep(new PassThroughStringLocalizer<ReadMediaFileStep>()), settings, Services(new InMemoryMediaStore()));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    private static ServiceProvider Services(InMemoryMediaStore store, bool authorized = true)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDataSourcesCore();
        services.AddSingleton<IMediaFileStore>(store);
        services.Configure<MediaOptions>(options => options.AllowedFileExtensions = [".csv", ".xlsx", ".json", ".zip"]);

        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        services.AddSingleton(authorization.Object);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// A media library held in memory, with public URLs under /media.
    /// </summary>
    private sealed class InMemoryMediaStore : IMediaFileStore
    {
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void Write(string path, string content) => Files[path] = Encoding.UTF8.GetBytes(content);

        public string Read(string path) => Encoding.UTF8.GetString(Files[path]);

        public IFileStoreCapabilities Capabilities => new FileStoreCapabilities(false, false);

        public string MapPathToPublicUrl(string path) => "/media/" + path;

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
            => Task.FromResult<IFileStoreEntry>(Files.TryGetValue(path, out var content) ? new Entry(path, content.Length) : null);

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path) => Task.FromResult<IFileStoreEntry>(null);

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            await Task.CompletedTask;

            yield break;
        }

        public Task<bool> TryCreateDirectoryAsync(string path) => Task.FromResult(true);

        public Task<bool> TryDeleteFileAsync(string path) => Task.FromResult(Files.Remove(path));

        public Task<bool> TryDeleteDirectoryAsync(string path) => Task.FromResult(false);

        public Task MoveFileAsync(string oldPath, string newPath) => throw new NotSupportedException();

        public Task CopyFileAsync(string srcPath, string dstPath) => throw new NotSupportedException();

        public Task<Stream> GetFileStreamAsync(string path)
            => Task.FromResult<Stream>(new MemoryStream(Files[path], writable: false));

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry) => GetFileStreamAsync(fileStoreEntry.Path);

        public async Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            if (!overwrite && Files.ContainsKey(path))
            {
                throw new FileStoreException("The file exists.");
            }

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory);
            Files[path] = memory.ToArray();

            return path;
        }

        private sealed class Entry : IFileStoreEntry
        {
            public Entry(string path, long length)
            {
                Path = path;
                Length = length;
            }

            public string Path { get; }

            public string Name => System.IO.Path.GetFileName(Path);

            public string DirectoryPath => System.IO.Path.GetDirectoryName(Path)?.Replace('\\', '/');

            public long Length { get; }

            public DateTime LastModifiedUtc => DateTime.UtcNow;

            public bool IsDirectory => false;
        }
    }
}
