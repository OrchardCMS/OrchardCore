using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.FileStorage;

public sealed class DefaultTempDirectoryProviderTests : IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        nameof(DefaultTempDirectoryProviderTests),
        Guid.NewGuid().ToString("N"));

    private DefaultTempDirectoryProvider CreateStore(string tenantName = "TestTenant", string tempPath = null)
        => new(
            Options.Create(new TempDirectoryOptions { Path = tempPath ?? _basePath }),
            new ShellSettings { Name = tenantName });

    [Fact]
    public void GetRootDirectory_ReturnsTenantScopedPath_AndCreatesIt()
    {
        var store = CreateStore("Alpha");

        var root = store.GetRootDirectory();

        Assert.Equal(Path.Combine(_basePath, "Alpha"), root);
        Assert.True(Directory.Exists(root));
    }

    [Fact]
    public void GetRootDirectory_OnUnix_CreatesWithOwnerOnlyPermissions()
    {
        if (!OperatingSystem.IsWindows())
        {
            var root = CreateStore("Alpha").GetRootDirectory();

            Assert.Equal(
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(root));

            return;
        }

        Assert.Skip("Unix file permissions are not supported on Windows.");
    }

    [Fact]
    public void DifferentTenants_GetIsolatedRoots()
    {
        var alpha = CreateStore("Alpha").GetRootDirectory();
        var beta = CreateStore("Beta").GetRootDirectory();

        Assert.NotEqual(alpha, beta);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetRootDirectory_WithoutConfiguredPath_UsesUserDirectoryWithoutWarning(string configuredPath)
    {
        var localAppDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppDataPath))
        {
            Assert.Skip("No local application data directory is available.");
        }

        var tenantName = Guid.NewGuid().ToString("N");
        var logger = new Mock<ILogger<DefaultTempDirectoryProvider>>(MockBehavior.Strict);
        var store = new DefaultTempDirectoryProvider(
            Options.Create(new TempDirectoryOptions { Path = configuredPath }),
            new ShellSettings { Name = tenantName },
            logger.Object);
        var root = store.GetRootDirectory();

        try
        {
            Assert.Equal(Path.Combine(localAppDataPath, "OrchardCore", "Temp", tenantName), root);
            Assert.Equal(root, store.GetRootDirectory());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetDefaultBasePath_WithUserDirectory_ReturnsStablePathWithoutWarning()
    {
        var logger = new Mock<ILogger<DefaultTempDirectoryProvider>>(MockBehavior.Strict);
        var path = DefaultTempDirectoryProvider.GetDefaultBasePath(_basePath, logger.Object);

        Assert.Equal(Path.Combine(_basePath, "OrchardCore", "Temp"), path);
        Assert.Equal(path, DefaultTempDirectoryProvider.GetDefaultBasePath(_basePath, logger.Object));
        Assert.True(Directory.Exists(path));

        if (!OperatingSystem.IsWindows())
        {
            Assert.Equal(
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(path));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetDefaultBasePath_WithoutUserDirectory_CreatesUniqueDirectoryAndWarns(string localAppDataPath)
    {
        AssertFallbackDirectory(localAppDataPath);
    }

    [Fact]
    public void GetDefaultBasePath_WithUnusableUserDirectory_CreatesUniqueDirectoryAndWarns()
    {
        Directory.CreateDirectory(_basePath);
        var filePath = Path.Combine(_basePath, "NotADirectory");
        File.WriteAllText(filePath, "test");

        AssertFallbackDirectory(filePath);
    }

    private static void AssertFallbackDirectory(string localAppDataPath)
    {
        var logger = new Mock<ILogger<DefaultTempDirectoryProvider>>();
        var tempDirectory = DefaultTempDirectoryProvider.GetDefaultBasePath(localAppDataPath, logger.Object);
        try
        {
            Assert.StartsWith("OrchardCore-", Path.GetFileName(tempDirectory));
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(Path.GetTempPath()),
                Directory.GetParent(tempDirectory).FullName);
            Assert.True(Directory.Exists(tempDirectory));

            logger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains(tempDirectory)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            if (!OperatingSystem.IsWindows())
            {
                var ownerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
                Assert.Equal(ownerOnly, File.GetUnixFileMode(tempDirectory));
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void GetTempFileName_ReturnsUniquePathsUnderRoot_WithoutCreatingFile()
    {
        var store = CreateStore();

        var first = store.GetTempFileName();
        var second = store.GetTempFileName();

        Assert.StartsWith(Path.Combine(_basePath, "TestTenant"), first);
        Assert.NotEqual(first, second);
        Assert.False(File.Exists(first));
        // The root is ensured to exist even though the file is not created.
        Assert.True(Directory.Exists(Path.GetDirectoryName(first)));
    }

    [Theory]
    [InlineData(".zip", ".zip")]
    [InlineData("zip", ".zip")]
    public void GetTempFileName_HonorsExtension(string extension, string expectedExtension)
    {
        var store = CreateStore();

        var path = store.GetTempFileName(extension);

        Assert.Equal(expectedExtension, Path.GetExtension(path));
    }

    [Fact]
    public void CreateTempSubdirectory_CreatesUniqueExistingDirectories()
    {
        var store = CreateStore();

        var first = store.CreateTempSubdirectory();
        var second = store.CreateTempSubdirectory();

        Assert.NotEqual(first, second);
        Assert.True(Directory.Exists(first));
        Assert.True(Directory.Exists(second));
        Assert.StartsWith(Path.Combine(_basePath, "TestTenant"), first);
    }

    [Fact]
    public void CreateTempSubdirectory_AppliesPrefixToDirectoryName()
    {
        var store = CreateStore();

        var path = store.CreateTempSubdirectory("upload-");

        Assert.True(Directory.Exists(path));
        Assert.Equal(Path.Combine(_basePath, "TestTenant"), Path.GetDirectoryName(path));
        Assert.StartsWith("upload-", Path.GetFileName(path));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }
}
