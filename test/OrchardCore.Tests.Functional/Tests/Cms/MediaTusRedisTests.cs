using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaTusRedisTests : IAsyncLifetime
{
    private const int SmallFileSize = 1 * 1024 * 1024;   // 1 MB
    private const int LargeFileSize = 10 * 1024 * 1024;  // 10 MB

    private readonly CmsSetupFixture _fixture;
    private TenantInfo _tenant;

    public MediaTusRedisTests(CmsSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("MediaTusRedis");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync()
    {
        MediaHelper.CleanupTestFiles();
        return ValueTask.CompletedTask;
    }

    [RedisFact]
    public async Task ShouldUploadFileViaTusWithRedis()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("tus-redis-upload.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "tus-redis-upload.jpg");

        await page.CloseAsync();
    }

    [RedisFact]
    public async Task ShouldPauseAndResumeWithRedis()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        await ThrottleTusUploadsAsync(page);
        var filePath = MediaHelper.GenerateTestFile("tus-redis-pause.jpg", LargeFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);

        // Wait for the upload toast to show the file in-progress.
        var container = page.Locator(".upload-toast-container");
        await container.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        var fileRow = container.Locator(".upload-toast-item").Filter(new() { HasText = "tus-redis-pause.jpg" });
        await fileRow.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });

        // Pause the upload.
        var pauseBtn = fileRow.Locator("button:has(svg[data-icon=\"pause\"])");
        await Assertions.Expect(pauseBtn).ToBeVisibleAsync();
        await pauseBtn.ClickAsync();
        await Assertions.Expect(fileRow.Locator("svg[data-icon=\"play\"]")).ToBeVisibleAsync();

        // Resume the upload.
        await fileRow.Locator("button:has(svg[data-icon=\"play\"])").ClickAsync();
        await Assertions.Expect(fileRow.Locator("svg[data-icon=\"pause\"]")).ToBeVisibleAsync();

        // Remove throttle and verify a fresh upload completes.
        await page.UnrouteAsync("**/*api/media/tus*");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");
        var verifyFile = MediaHelper.GenerateTestFile("tus-redis-verify.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, verifyFile);
        await MediaHelper.ExpectFileInLibraryAsync(page, "tus-redis-verify.jpg");

        await page.CloseAsync();
    }

    private static async Task ThrottleTusUploadsAsync(IPage page, int delayMs = 3000)
    {
        await page.RouteAsync("**/*api/media/tus*", async route =>
        {
            if (route.Request.Method == "PATCH")
            {
                await Task.Delay(delayMs);
                await route.ContinueAsync();
            }
            else
            {
                await route.ContinueAsync();
            }
        });
    }
}
