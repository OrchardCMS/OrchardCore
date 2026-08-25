using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.FileStorage;

public sealed class DefaultTempWorkspaceTests : IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        nameof(DefaultTempWorkspaceTests),
        Guid.NewGuid().ToString("N"));

    private DefaultTempWorkspace CreateStore(string tenantName = "TestTenant", string tempPath = null)
        => new(
            Options.Create(new TempWorkspaceOptions { TempPath = tempPath ?? _basePath }),
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
    public void DifferentTenants_GetIsolatedRoots()
    {
        var alpha = CreateStore("Alpha").GetRootDirectory();
        var beta = CreateStore("Beta").GetRootDirectory();

        Assert.NotEqual(alpha, beta);
    }

    [Fact]
    public void EmptyTempPath_FallsBackToSystemTempPath()
    {
        var store = new DefaultTempWorkspace(
            Options.Create(new TempWorkspaceOptions { TempPath = null }),
            new ShellSettings { Name = "Gamma" });

        var root = store.GetRootDirectory();

        Assert.Equal(Path.Combine(Path.GetTempPath(), "Gamma"), root);

        // Cleanup this one since it is created outside _basePath.
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void GetOrCreateSubdirectory_CreatesNestedDirectoryUnderRoot()
    {
        var store = CreateStore();

        var sub = store.GetOrCreateSubdirectory("ChunkedFileUploads");

        Assert.Equal(Path.Combine(_basePath, "TestTenant", "ChunkedFileUploads"), sub);
        Assert.True(Directory.Exists(sub));
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("../../escape")]
    [InlineData("nested/../../escape")]
    public void GetOrCreateSubdirectory_RejectsPathTraversal(string name)
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() => store.GetOrCreateSubdirectory(name));
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

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }
}
