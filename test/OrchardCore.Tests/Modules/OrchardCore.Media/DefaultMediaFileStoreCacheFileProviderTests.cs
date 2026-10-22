using OrchardCore.FileStorage;
using OrchardCore.Media.Core;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

/// <summary>
/// Verifies that the local media cache can mirror any remote media path, including folder
/// names that are invalid on the local file system, e.g. 'test:asdf' on Windows/NTFS.
/// See https://github.com/OrchardCMS/OrchardCore/issues/17644.
/// These tests run on every platform; the CI Windows runners exercise the NTFS behavior.
/// </summary>
public class DefaultMediaFileStoreCacheFileProviderTests : IDisposable
{
    private readonly string _root;
    private readonly DefaultMediaFileStoreCacheFileProvider _provider;

    public DefaultMediaFileStoreCacheFileProviderTests()
    {
        _root = Directory.CreateTempSubdirectory("ms-cache-tests").FullName;
        _provider = new DefaultMediaFileStoreCacheFileProvider(
            NullLogger<DefaultMediaFileStoreCacheFileProvider>.Instance,
            "/media",
            _root);
    }

    public void Dispose()
    {
        _provider.Dispose();
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private async Task SetCacheAsync(string path, string content = "cached content")
    {
        var entry = Mock.Of<IFileStoreEntry>(e => e.Path == path);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _provider.SetCacheAsync(stream, entry, CancellationToken.None);
    }

    [Theory]
    [InlineData("test:asdf/pic.png")] // Issue #17644.
    [InlineData("weird*folder/what?.png")]
    [InlineData("quote\"pipe|/less<more>.png")]
    [InlineData("trailing./file.png")]
    [InlineData("con/nul.txt")]
    [InlineData("100%/50%off.png")]
    public async Task SetCache_NtfsInvalidPath_CachesAndServesFile(string path)
    {
        await SetCacheAsync(path, "hello");

        Assert.True(await _provider.IsCachedAsync(path));

        // The resolver middleware and the static file middleware use a leading slash.
        Assert.True(await _provider.IsCachedAsync('/' + path));

        var fileInfo = ((IFileProvider)_provider).GetFileInfo('/' + path);
        Assert.True(fileInfo.Exists);

        using var reader = new StreamReader(fileInfo.CreateReadStream());
        Assert.Equal("hello", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SetCache_NtfsInvalidPath_EscapesPhysicalLocation()
    {
        await SetCacheAsync("test:asdf/pic.png");

        var directories = Directory.GetDirectories(_root).Select(Path.GetFileName).ToList();

        Assert.Contains("test%3Aasdf", directories);
        // Even on file systems that would allow it, the raw name must not be used.
        Assert.DoesNotContain("test:asdf", directories);
    }

    [Fact]
    public async Task GetDirectoryContents_NtfsInvalidPath_ReturnsTraversableLogicalName()
    {
        await SetCacheAsync("test:asdf/pic.png");

        var directory = Assert.Single(_provider.GetDirectoryContents(string.Empty));

        Assert.Equal("test:asdf", directory.Name);
        Assert.True(_provider.GetFileInfo(directory.Name + "/pic.png").Exists);
    }

    [Fact]
    public async Task SetCache_ValidPath_KeepsVerbatimLayout()
    {
        await SetCacheAsync("folder/pic.png");

        Assert.True(File.Exists(Path.Combine(_root, "folder", "pic.png")));
    }

    [Fact]
    public async Task IsCached_UncachedNtfsInvalidPath_ReturnsFalse()
    {
        await SetCacheAsync("test:asdf/pic.png");

        Assert.False(await _provider.IsCachedAsync("missing:folder/none.png"));
    }

    [Fact]
    public async Task TryDeleteFile_NtfsInvalidPath_DeletesCachedFile()
    {
        await SetCacheAsync("test:asdf/pic.png");

        Assert.True(await _provider.TryDeleteFileAsync("test:asdf/pic.png"));
        Assert.False(await _provider.IsCachedAsync("test:asdf/pic.png"));
        Assert.False(await _provider.TryDeleteFileAsync("test:asdf/pic.png"));
    }

    [Fact]
    public async Task TryDeleteDirectory_NtfsInvalidPath_DeletesCachedDirectory()
    {
        await SetCacheAsync("test:asdf/pic.png");

        Assert.True(await _provider.TryDeleteDirectoryAsync("test:asdf"));
        Assert.False(await _provider.IsCachedAsync("test:asdf/pic.png"));
    }

    [Fact]
    public async Task Purge_WithNtfsInvalidPaths_RemovesEverything()
    {
        await SetCacheAsync("test:asdf/pic.png");
        await SetCacheAsync("normal/pic.png");

        var hasErrors = await _provider.PurgeAsync();

        Assert.False(hasErrors);
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public async Task CachedPaths_UnescapedRelativePhysicalPath_RoundTripsToMediaPath()
    {
        // Mirrors how RemoteMediaCacheBackgroundTask maps physical cache paths back to
        // remote media paths when cleaning stale entries.
        var paths = new[] { "test:asdf/pic.png", "folder/pic.png", "100%/50%off.png" };
        foreach (var path in paths)
        {
            await SetCacheAsync(path);
        }

        var roundTripped = Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
            .Select(file => MediaCachePathEscaper
                .Unescape(Path.GetRelativePath(_root, file))
                .Replace(Path.DirectorySeparatorChar, '/'))
            .ToList();

        foreach (var path in paths)
        {
            Assert.Contains(path, roundTripped);
        }
    }
}
