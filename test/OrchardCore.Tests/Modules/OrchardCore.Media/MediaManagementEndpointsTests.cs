using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaManagementEndpointsTests
{
    [Fact]
    public async Task DeleteAllAsync_MissingPath_ContinuesAndReportsIt()
    {
        var store = new Mock<IMediaFileStore>();
        store.Setup(value => value.TryDeleteFileAsync("first.png")).ReturnsAsync(true);
        store.Setup(value => value.TryDeleteFileAsync("missing.png")).ReturnsAsync(false);
        store.Setup(value => value.TryDeleteFileAsync("last.png")).ReturnsAsync(true);

        var result = await DeleteMediaListEndpoint.DeleteAllAsync(store.Object, ["first.png", "missing.png", "last.png"]);

        Assert.Equal(["first.png", "last.png"], result.Paths);
        Assert.Equal(["missing.png"], result.MissingPaths);
        store.Verify(value => value.TryDeleteFileAsync("last.png"), Times.Once);
    }

    [Fact]
    public async Task BufferToTemporaryFileAsync_Stream_CopiesContentAndDeletesTheFileOnDispose()
    {
        using var source = new MemoryStream([1, 2, 3]);

        string path;
        await using (var buffer = await UploadMediaEndpoint.BufferToTemporaryFileAsync(source, TestContext.Current.CancellationToken))
        {
            path = buffer.Name;

            Assert.Equal(3, buffer.Length);
            Assert.Equal(0, buffer.Position);
            Assert.Equal(1, buffer.ReadByte());
            Assert.True(File.Exists(path));
        }

        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task GetFilesSnapshotAsync_FirstPage_AlwaysWalks()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var walks = 0;

        await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, "key", 0, () => WalkAsync(ref walks));
        await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, "key", 0, () => WalkAsync(ref walks));

        Assert.Equal(2, walks);
    }

    [Fact]
    public async Task GetFilesSnapshotAsync_NextPages_ReuseTheWalk()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var walks = 0;

        var first = await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, "key", 0, () => WalkAsync(ref walks));
        var second = await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, "key", 50, () => WalkAsync(ref walks));
        var otherCaller = await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, "other", 50, () => WalkAsync(ref walks));

        Assert.Same(first, second);
        Assert.NotSame(first, otherCaller);
        Assert.Equal(2, walks);
    }

    [Fact]
    public async Task GetFilesSnapshotAsync_NoCallerIdentifier_NeverCaches()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var walks = 0;

        await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, null, 0, () => WalkAsync(ref walks));
        await GetAllMediaItemsEndpoint.GetFilesSnapshotAsync(cache, null, 50, () => WalkAsync(ref walks));

        Assert.Equal(2, walks);
        Assert.Equal(0, cache.Count);
    }

    private static Task<List<FileStoreEntryDto>> WalkAsync(ref int walks)
    {
        walks++;

        return Task.FromResult(new List<FileStoreEntryDto>());
    }
}
