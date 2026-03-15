using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class MediaHelper
{
    private static readonly string _tempDir = Path.Combine(Path.GetTempPath(), "playwright-test-files");

    /// <summary>
    /// Generates a test file of a given size filled with 'A' bytes.
    /// Returns the absolute path to the generated file.
    /// </summary>
    public static string GenerateTestFile(string name, int sizeInBytes)
    {
        if (!Directory.Exists(_tempDir))
        {
            Directory.CreateDirectory(_tempDir);
        }

        var filePath = Path.Combine(_tempDir, name);
        const int chunkSize = 1024 * 1024;

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        var remaining = sizeInBytes;

        while (remaining > 0)
        {
            var size = Math.Min(chunkSize, remaining);
            var buffer = new byte[size];
            Array.Fill(buffer, (byte)0x41);
            fs.Write(buffer, 0, size);
            remaining -= size;
        }

        return filePath;
    }

    /// <summary>
    /// Cleans up all generated test files.
    /// </summary>
    public static void CleanupTestFiles()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    /// <summary>
    /// Navigate to the Media admin page and wait for the Vue app to be fully mounted.
    /// </summary>
    public static async Task NavigateToMediaAsync(IPage page, string prefix = "")
    {
        await page.GotoAsync($"{prefix}/Admin/Media");
        // Wait for the Vue app to mount and the upload button to become visible,
        // which indicates that permissions have loaded and the app is ready.
        await page.Locator(".fileinput-button").First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 30_000 });
    }

    /// <summary>
    /// Upload a file using the file input.
    /// </summary>
    public static async Task UploadFileAsync(IPage page, string filePath)
    {
        var fileInput = page.Locator("#fileupload");
        await fileInput.SetInputFilesAsync(filePath);
    }

    /// <summary>
    /// Upload multiple files at once using the file input.
    /// </summary>
    public static async Task UploadFilesAsync(IPage page, params string[] filePaths)
    {
        var fileInput = page.Locator("#fileupload");
        await fileInput.SetInputFilesAsync(filePaths);
    }

    /// <summary>
    /// Check that a file appears in the media library file list.
    /// </summary>
    public static async Task ExpectFileInLibraryAsync(IPage page, string fileName, int timeoutMs = 30_000)
    {
        var locator = page.GetByText(fileName, new() { Exact = true }).First;
        try
        {
            await Assertions.Expect(locator).ToBeVisibleAsync(new() { Timeout = timeoutMs / 2 });
        }
        catch
        {
            await page.ReloadAsync();
            await page.Locator(".fileinput-button").First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 30_000 });
            await Assertions.Expect(locator).ToBeVisibleAsync(new() { Timeout = timeoutMs / 2 });
        }
    }

    /// <summary>
    /// Check that a file does NOT appear in the media library file list.
    /// </summary>
    public static async Task ExpectFileNotInLibraryAsync(IPage page, string fileName, int timeoutMs = 5_000)
    {
        var locator = page.GetByText(fileName, new() { Exact = true }).First;
        await Assertions.Expect(locator).Not.ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>
    /// Check that a folder appears in the folder tree.
    /// </summary>
    public static async Task ExpectFolderInTreeAsync(IPage page, string folderName, int timeoutMs = 10_000)
    {
        var locator = page.Locator("#folder-tree .folder-name").GetByText(folderName, new() { Exact = true });
        await Assertions.Expect(locator.First).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>
    /// Create a subfolder using the folder tree action menu on the root folder.
    /// </summary>
    public static async Task CreateFolderAsync(IPage page, string folderName)
    {
        // Click the action button on the root folder (first folder with treeroot class).
        var rootFolder = page.Locator("#folder-tree .treeroot").First;
        await rootFolder.HoverAsync();
        await rootFolder.Locator(".action-button").ClickAsync();

        // Wait for the folder action modal to appear.
        var modal = page.Locator(".action-modal").First;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Select "Create SubFolder" radio if not already selected.
        var createRadio = modal.Locator("input[type='radio']").First;
        await createRadio.CheckAsync();

        // Type folder name.
        var input = modal.Locator("input[name='create-folder']");
        await input.FillAsync(folderName);

        // Submit.
        await modal.Locator("#btn-submit").ClickAsync();

        // Wait for the folder to appear in the tree.
        await ExpectFolderInTreeAsync(page, folderName);
    }

    /// <summary>
    /// Navigate into a folder by clicking it in the folder tree.
    /// </summary>
    public static async Task NavigateToFolderAsync(IPage page, string folderName)
    {
        // Expand the root folder first.
        var rootExpand = page.Locator("#folder-tree .treeroot .expand").First;
        try
        {
            await rootExpand.ClickAsync(new() { Timeout = 3_000 });
            await page.WaitForTimeoutAsync(500);
        }
        catch
        {
            // Already expanded.
        }

        // Click the folder link in the tree.
        var folderLink = page.Locator("#folder-tree .folder-menu-item")
            .Filter(new() { Has = page.Locator(".folder-name", new() { HasTextString = folderName }) });
        await folderLink.First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Open the action menu for a specific file.
    /// </summary>
    public static async Task OpenFileActionMenuAsync(IPage page, string fileName)
    {
        // Find the file row containing the file name and click its action button.
        var fileRow = page.Locator(".table-row.file-item")
            .Filter(new() { HasTextString = fileName });
        await fileRow.First.Locator(".action-button").ClickAsync();

        // Wait for the popup menu to appear.
        await page.Locator("#overlay_menu").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
    }

    /// <summary>
    /// Delete a file via its action menu.
    /// </summary>
    public static async Task DeleteFileAsync(IPage page, string fileName)
    {
        await OpenFileActionMenuAsync(page, fileName);

        // Click "Delete" in the popup menu.
        await page.Locator("#overlay_menu").GetByText("Delete").ClickAsync();

        // Wait for confirm modal and click the confirm button.
        var modal = page.Locator(".action-modal").First;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
        await modal.Locator(".ma-btn-primary").ClickAsync();

        // Wait for the file to disappear.
        await page.WaitForTimeoutAsync(1_000);
    }

    /// <summary>
    /// Rename a file via its action menu.
    /// </summary>
    public static async Task RenameFileAsync(IPage page, string oldName, string newName)
    {
        await OpenFileActionMenuAsync(page, oldName);

        // Click "Rename" in the popup menu.
        await page.Locator("#overlay_menu").GetByText("Rename").ClickAsync();

        // Wait for the file action modal.
        var modal = page.Locator(".action-modal").Last;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Clear and fill the rename input.
        var input = modal.Locator("input[name='rename']");
        await input.ClearAsync();
        await input.FillAsync(newName);

        // Submit.
        await modal.Locator("#btn-submit").ClickAsync();
        await page.WaitForTimeoutAsync(1_000);
    }

    /// <summary>
    /// Move a file to a target folder via its action menu.
    /// </summary>
    public static async Task MoveFileAsync(IPage page, string fileName, string targetFolder)
    {
        await OpenFileActionMenuAsync(page, fileName);

        // Click "Move" in the popup menu.
        await page.Locator("#overlay_menu").GetByText("Move").ClickAsync();

        // Wait for the file action modal with TreeSelect.
        var modal = page.Locator(".action-modal").Last;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Click the TreeSelect to open the dropdown.
        await modal.Locator(".p-treeselect").ClickAsync();

        // Select the target folder from the TreeSelect overlay.
        await SelectTreeSelectFolderAsync(page, targetFolder);

        // Submit.
        await modal.Locator("#btn-submit").ClickAsync();
        await page.WaitForTimeoutAsync(1_000);
    }

    /// <summary>
    /// Copy a file to a target folder via its action menu.
    /// </summary>
    public static async Task CopyFileAsync(IPage page, string fileName, string targetFolder)
    {
        await OpenFileActionMenuAsync(page, fileName);

        // Click "Copy" in the popup menu.
        await page.Locator("#overlay_menu").GetByText("Copy").ClickAsync();

        // Wait for the file action modal with TreeSelect.
        var modal = page.Locator(".action-modal").Last;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Click the TreeSelect to open the dropdown.
        await modal.Locator(".p-treeselect").ClickAsync();

        // Select the target folder from the TreeSelect overlay.
        await SelectTreeSelectFolderAsync(page, targetFolder);

        // Submit.
        await modal.Locator("#btn-submit").ClickAsync();
        await page.WaitForTimeoutAsync(1_000);
    }

    /// <summary>
    /// Select a folder in an open PrimeVue TreeSelect overlay.
    /// Expands the root "Files" node if needed, then clicks the target folder.
    /// </summary>
    private static async Task SelectTreeSelectFolderAsync(IPage page, string folderName)
    {
        var overlay = page.Locator(".p-treeselect-overlay");
        await overlay.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Expand the root "Files" node so child folders become visible.
        var rootToggle = overlay.Locator(".p-tree-node-toggle-button").First;
        await rootToggle.ClickAsync();
        await page.WaitForTimeoutAsync(300);

        // Click the target folder.
        await overlay.GetByText(folderName, new() { Exact = true }).First.ClickAsync();
    }
}
