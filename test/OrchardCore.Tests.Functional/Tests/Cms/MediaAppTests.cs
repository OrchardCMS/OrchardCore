using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class MediaAppTests : IClassFixture<SaasFixture>, IAsyncLifetime
{
    private const int SmallFileSize = 100 * 1024; // 100 KB

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaAppTests(SaasFixture fixture)
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
    public async Task ShouldUploadSingleFile()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("single-upload.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "single-upload.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldUploadMultipleFiles()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var file1 = MediaHelper.GenerateTestFile("multi-upload-1.jpg", SmallFileSize);
        var file2 = MediaHelper.GenerateTestFile("multi-upload-2.jpg", SmallFileSize);
        await MediaHelper.UploadFilesAsync(page, file1, file2);

        await MediaHelper.ExpectFileInLibraryAsync(page, "multi-upload-1.jpg");
        await MediaHelper.ExpectFileInLibraryAsync(page, "multi-upload-2.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldCreateFolder()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        await MediaHelper.CreateFolderAsync(page, "TestFolder");
        await MediaHelper.ExpectFolderInTreeAsync(page, "TestFolder");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldDeleteSingleFile()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("delete-me.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "delete-me.jpg");

        await MediaHelper.DeleteFileAsync(page, "delete-me.jpg");
        await MediaHelper.ExpectFileNotInLibraryAsync(page, "delete-me.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldRenameFile()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        var filePath = MediaHelper.GenerateTestFile("before-rename.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "before-rename.jpg");

        await MediaHelper.RenameFileAsync(page, "before-rename.jpg", "after-rename.jpg");

        await MediaHelper.ExpectFileInLibraryAsync(page, "after-rename.jpg");
        await MediaHelper.ExpectFileNotInLibraryAsync(page, "before-rename.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldMoveFileToFolder()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        // Create target folder.
        await MediaHelper.CreateFolderAsync(page, "MoveTarget");

        // Upload a file to root.
        var filePath = MediaHelper.GenerateTestFile("move-me.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "move-me.jpg");

        // Move the file to the target folder.
        await MediaHelper.MoveFileAsync(page, "move-me.jpg", "MoveTarget");

        // File should no longer be in root.
        await MediaHelper.ExpectFileNotInLibraryAsync(page, "move-me.jpg");

        // Navigate to the target folder and verify the file is there.
        await MediaHelper.NavigateToFolderAsync(page, "MoveTarget");
        await MediaHelper.ExpectFileInLibraryAsync(page, "move-me.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldCopyFileToFolder()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        // Create target folder.
        await MediaHelper.CreateFolderAsync(page, "CopyTarget");

        // Upload a file to root.
        var filePath = MediaHelper.GenerateTestFile("copy-me.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "copy-me.jpg");

        // Copy the file to the target folder.
        await MediaHelper.CopyFileAsync(page, "copy-me.jpg", "CopyTarget");

        // File should still be in root.
        await MediaHelper.ExpectFileInLibraryAsync(page, "copy-me.jpg");

        // Navigate to the target folder and verify the copy is there.
        await MediaHelper.NavigateToFolderAsync(page, "CopyTarget");
        await MediaHelper.ExpectFileInLibraryAsync(page, "copy-me.jpg");

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldNavigateFolderHierarchy()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        // Create a parent folder.
        await MediaHelper.CreateFolderAsync(page, "ParentFolder");

        // Navigate into the parent folder.
        await MediaHelper.NavigateToFolderAsync(page, "ParentFolder");

        // Verify breadcrumb shows the folder.
        var breadcrumb = page.Locator("#breadcrumb");
        await Assertions.Expect(breadcrumb.GetByText("ParentFolder")).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task ShouldUploadFileToSubfolder()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await MediaHelper.NavigateToMediaAsync(page, $"/{_tenant.Prefix}");

        // Create a folder and navigate into it.
        await MediaHelper.CreateFolderAsync(page, "SubfolderUpload");
        await MediaHelper.NavigateToFolderAsync(page, "SubfolderUpload");

        // Upload a file while in the subfolder.
        var filePath = MediaHelper.GenerateTestFile("subfolder-file.jpg", SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, "subfolder-file.jpg");

        await page.CloseAsync();
    }
}
