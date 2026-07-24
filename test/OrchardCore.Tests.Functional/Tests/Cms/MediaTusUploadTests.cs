using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaTusUploadTests : IAsyncLifetime
{
    private const int SmallFileSize = 1 * 1024 * 1024;   // 1 MB
    private const int LargeFileSize = 10 * 1024 * 1024;  // 10 MB

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaTusUploadTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("MediaTus");
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
    public async Task ShouldUploadFileViaTus()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("tus-upload-test.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "tus-upload-test.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldPauseAndResumeSingleFileUpload()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        await ThrottleTusUploadsAsync(page);
        var filePath = MediaHelper.GenerateTestFile("pause-resume-test.jpg", LargeFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);

        // Wait for the upload toast to show the file in-progress.
        var container = page.Locator(".upload-toast-container");
        await container.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        var fileRow = container.Locator(".upload-toast-item").Filter(new() { HasText = "pause-resume-test.jpg" });
        await fileRow.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });

        // Verify the pause button is visible (upload is active).
        var pauseBtn = fileRow.Locator("button:has(svg[data-icon=\"pause\"])");
        await Assertions.Expect(pauseBtn).ToBeVisibleAsync();

        // Pause the upload — icon should switch to play.
        await pauseBtn.ClickAsync();
        await Assertions.Expect(fileRow.Locator("svg[data-icon=\"play\"]")).ToBeVisibleAsync();

        // Resume the upload — icon should switch back to pause.
        await fileRow.Locator("button:has(svg[data-icon=\"play\"])").ClickAsync();
        await Assertions.Expect(fileRow.Locator("svg[data-icon=\"pause\"]")).ToBeVisibleAsync();

        // Remove throttle and verify a fresh upload completes.
        await page.UnrouteAllAsync(new() { Behavior = UnrouteBehavior.Wait });
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");
        var filePath2 = MediaHelper.GenerateTestFile("pause-resume-verify.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath2);
        await MediaHelper.ExpectFileInLibraryAsync(page, "pause-resume-verify.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldPauseAndResumeMultipleSimultaneousUploads()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        await ThrottleTusUploadsAsync(page);
        var filePath1 = MediaHelper.GenerateTestFile("multi-pause-1.jpg", LargeFileSize);
        var filePath2 = MediaHelper.GenerateTestFile("multi-pause-2.jpg", LargeFileSize);

        var fileInput = page.Locator("#fileupload");
        await fileInput.SetInputFilesAsync(new[] { filePath1, filePath2 });

        var container = page.Locator(".upload-toast-container");
        await container.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        var row1 = container.Locator(".upload-toast-item").Filter(new() { HasText = "multi-pause-1.jpg" });
        var row2 = container.Locator(".upload-toast-item").Filter(new() { HasText = "multi-pause-2.jpg" });
        await row1.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });
        await row2.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });

        // Pause both uploads.
        await row1.Locator("button:has(svg[data-icon=\"pause\"])").ClickAsync();
        await row2.Locator("button:has(svg[data-icon=\"pause\"])").ClickAsync();
        await Assertions.Expect(row1.Locator("svg[data-icon=\"play\"]")).ToBeVisibleAsync();
        await Assertions.Expect(row2.Locator("svg[data-icon=\"play\"]")).ToBeVisibleAsync();

        // Resume both uploads.
        await row1.Locator("button:has(svg[data-icon=\"play\"])").ClickAsync();
        await row2.Locator("button:has(svg[data-icon=\"play\"])").ClickAsync();
        await Assertions.Expect(row1.Locator("svg[data-icon=\"pause\"]")).ToBeVisibleAsync();
        await Assertions.Expect(row2.Locator("svg[data-icon=\"pause\"]")).ToBeVisibleAsync();

        // Remove throttle and verify a fresh upload completes.
        await page.UnrouteAllAsync(new() { Behavior = UnrouteBehavior.Wait });
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");
        var verifyFile = MediaHelper.GenerateTestFile("multi-verify.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, verifyFile);
        await MediaHelper.ExpectFileInLibraryAsync(page, "multi-verify.jpg");

        await page.CloseAsync();
    }

    /// <summary>
    /// Throttle TUS PATCH responses to keep uploads visibly in-progress.
    /// Delays the response so the client waits before sending the next chunk.
    /// </summary>
    private static async Task ThrottleTusUploadsAsync(IPage page, int delayMs = 3000)
    {
        // Intercept all requests; filter for TUS PATCH uploads in the handler.
        await page.RouteAsync("**/*", async route =>
        {
            if (route.Request.Url.Contains("/api/media/tus") && route.Request.Method == "PATCH")
            {
                await Task.Delay(delayMs);
            }

            await route.ContinueAsync();
        });
    }
}
