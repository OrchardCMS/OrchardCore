using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

/// <summary>
/// Checks what a media change costs the clients that did not cause it.
///
/// Every MediaChanged broadcast used to make each connected client reload its directory, so a single
/// upload cost one directory listing per open gallery — plus a HasChildren probe per subfolder, and, with
/// Secure Media, an authorization check per folder on top. The broadcast now carries the affected entry
/// so clients patch their store instead. These tests hold that line: a change must reach the other client
/// without it asking the server anything.
/// </summary>
[Collection(CmsTestCollection.Name)]
public sealed class MediaRealtimeTests : IAsyncLifetime
{
    private const string DirectoryContentRoute = "api/media/GetDirectoryContent";

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaRealtimeTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("Media");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync()
    {
        MediaHelper.CleanupTestFiles();

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task AnUpload_ReachesAnotherClient_WithoutItReloadingTheDirectory()
    {
        var prefix = $"/{_tenant.Prefix}";

        // Two galleries open on the same folder, as two editors would have.
        var uploader = await _fixture.CreatePageAsync();
        var observer = await _fixture.CreatePageAsync();

        await uploader.LoginAsync(prefix);
        await observer.LoginAsync(prefix);

        await MediaHelper.NavigateToMediaAsync(uploader, prefix);
        await MediaHelper.NavigateToMediaAsync(observer, prefix);

        // Warm up: a first upload proves the broadcast reaches the observer, and lets its own initial
        // directory load finish before anything is counted.
        var warmUpName = $"realtime-warmup-{Guid.NewGuid():N}.png";
        await MediaHelper.UploadFileAsync(uploader, MediaHelper.GenerateTestFile(warmUpName, 1024));
        await MediaHelper.ExpectFileInLibraryAsync(observer, warmUpName);

        var reloads = CountDirectoryReloads(observer);

        var fileName = $"realtime-{Guid.NewGuid():N}.png";
        await MediaHelper.UploadFileAsync(uploader, MediaHelper.GenerateTestFile(fileName, 1024));
        await MediaHelper.ExpectFileInLibraryAsync(uploader, fileName);

        // The observer must learn about it from the broadcast alone.
        await MediaHelper.ExpectFileInLibraryAsync(observer, fileName);

        Assert.Equal(0, reloads.Count);

        await uploader.CloseAsync();
        await observer.CloseAsync();
    }

    [Fact]
    public async Task ADeletion_ReachesAnotherClient_WithoutItReloadingTheDirectory()
    {
        var prefix = $"/{_tenant.Prefix}";

        var owner = await _fixture.CreatePageAsync();
        var observer = await _fixture.CreatePageAsync();

        await owner.LoginAsync(prefix);
        await observer.LoginAsync(prefix);

        await MediaHelper.NavigateToMediaAsync(owner, prefix);
        await MediaHelper.NavigateToMediaAsync(observer, prefix);

        var fileName = $"realtime-delete-{Guid.NewGuid():N}.png";
        await MediaHelper.UploadFileAsync(owner, MediaHelper.GenerateTestFile(fileName, 1024));
        await MediaHelper.ExpectFileInLibraryAsync(owner, fileName);

        // Both see it before anything is counted; this also flushes the observer's initial load.
        await MediaHelper.ExpectFileInLibraryAsync(observer, fileName);

        var reloads = CountDirectoryReloads(observer);

        await MediaHelper.DeleteFileAsync(owner, fileName);
        await MediaHelper.ExpectFileNotInLibraryAsync(owner, fileName);

        await MediaHelper.ExpectFileNotInLibraryAsync(observer, fileName);

        Assert.Equal(0, reloads.Count);

        await owner.CloseAsync();
        await observer.CloseAsync();
    }

    private static RequestCounter CountDirectoryReloads(IPage page)
    {
        var counter = new RequestCounter();

        page.Request += (_, request) =>
        {
            if (request.Url.Contains(DirectoryContentRoute, StringComparison.OrdinalIgnoreCase))
            {
                counter.Increment();
            }
        };

        return counter;
    }

    private sealed class RequestCounter
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Increment() => Interlocked.Increment(ref _count);
    }
}
