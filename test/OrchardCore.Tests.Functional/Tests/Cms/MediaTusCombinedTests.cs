using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaTusCombinedTests : IAsyncLifetime
{
    private const int SmallFileSize = 1 * 1024 * 1024;   // 1 MB
    private const int LargeFileSize = 10 * 1024 * 1024;  // 10 MB

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaTusCombinedTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("MediaTusRedisAzure");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync()
    {
        MediaHelper.CleanupTestFiles();
        return ValueTask.CompletedTask;
    }

    [RedisAndAzuriteFact]
    public async Task ShouldUploadFileWithRedisAndAzure()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("tus-combined-upload.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "tus-combined-upload.jpg");

        await page.CloseAsync();
    }

    [RedisAndAzuriteFact]
    public async Task ShouldPauseAndResumeWithRedisAndAzure()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        await ThrottleTusUploadsAsync(page);
        var filePath = MediaHelper.GenerateTestFile("tus-combined-pause.jpg", LargeFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);

        // Wait for the upload toast to show the file in-progress.
        var container = page.Locator(".upload-toast-container");
        await container.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        var fileRow = container.Locator(".upload-toast-item").Filter(new() { HasText = "tus-combined-pause.jpg" });
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
        var verifyFile = MediaHelper.GenerateTestFile("tus-combined-verify.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, verifyFile);
        await MediaHelper.ExpectFileInLibraryAsync(page, "tus-combined-verify.jpg");

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
