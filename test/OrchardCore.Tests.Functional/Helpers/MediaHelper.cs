using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class MediaHelper
{
    private static readonly string _tempDir = Path.Combine(Path.GetTempPath(), "playwright-test-files");

    // Minimal valid 1x1 white JPEG (635 bytes). Generated files use this as a header
    // so ImageSharp can decode them without throwing UnknownImageFormatException.
    private static readonly byte[] _jpegHeader =
    [
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
        0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
        0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
        0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
        0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
        0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
        0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
        0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
        0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
        0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
        0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
        0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
        0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
        0x23, 0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24, 0x33, 0x62, 0x72,
        0x82, 0x09, 0x0A, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28,
        0x29, 0x2A, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x43, 0x44, 0x45,
        0x46, 0x47, 0x48, 0x49, 0x4A, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
        0x5A, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x73, 0x74, 0x75,
        0x76, 0x77, 0x78, 0x79, 0x7A, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
        0x8A, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0xA2, 0xA3,
        0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6,
        0xB7, 0xB8, 0xB9, 0xBA, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9,
        0xCA, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xE1, 0xE2,
        0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xF1, 0xF2, 0xF3, 0xF4,
        0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01,
        0x00, 0x00, 0x3F, 0x00, 0x7B, 0x94, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD9,
    ];

    /// <summary>
    /// Generates a valid JPEG test file of approximately the given size.
    /// The file starts with a minimal JPEG header so image processors can decode it.
    /// Returns the absolute path to the generated file.
    /// </summary>
    public static string GenerateTestFile(string name, int sizeInBytes)
    {
        if (!Directory.Exists(_tempDir))
        {
            Directory.CreateDirectory(_tempDir);
        }

        var filePath = Path.Combine(_tempDir, name);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        // Write the JPEG header (without the trailing EOI marker 0xFF 0xD9).
        var headerWithoutEoi = _jpegHeader.Length - 2;
        fs.Write(_jpegHeader, 0, headerWithoutEoi);

        // Pad to the requested size (minus 2 bytes for the EOI marker).
        var remaining = sizeInBytes - headerWithoutEoi - 2;
        if (remaining > 0)
        {
            const int chunkSize = 1024 * 1024;
            while (remaining > 0)
            {
                var size = Math.Min(chunkSize, remaining);
                var buffer = new byte[size];
                fs.Write(buffer, 0, size);
                remaining -= size;
            }
        }

        // Write the EOI marker.
        fs.Write(_jpegHeader, headerWithoutEoi, 2);

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
        await page.GotoAsync($"{prefix}/Admin/Media", new() { Timeout = 60_000 });
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

        // Wait for the popup menu and click "Create a subfolder".
        var menu = page.Locator("#overlay_folder_menu");
        await menu.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
        await menu.GetByText("Create a subfolder", new() { Exact = true }).ClickAsync();

        // Wait for the folder action modal to appear.
        var modal = page.Locator(".action-modal").First;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Type folder name (action is pre-selected from the menu, no radio needed).
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
        await page.Locator("#overlay_file_menu").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });
    }

    /// <summary>
    /// Delete a file via its action menu.
    /// </summary>
    public static async Task DeleteFileAsync(IPage page, string fileName)
    {
        await OpenFileActionMenuAsync(page, fileName);

        // Click "Delete" in the popup menu.
        await page.Locator("#overlay_file_menu").GetByText("Delete").ClickAsync();

        // Wait for confirm modal and click the confirm button.
        var modal = page.Locator(".action-modal").First;
        await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5_000 });

        // Keep compatibility with both legacy and updated modal button markup.
        var confirmButton = modal.Locator(".ma-btn-primary, #btn-submit, button:has-text('OK'), button:has-text('Ok'), button:has-text('Delete')").First;
        await confirmButton.ClickAsync();

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
        await page.Locator("#overlay_file_menu").GetByText("Rename").ClickAsync();

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
        await page.Locator("#overlay_file_menu").GetByText("Move").ClickAsync();

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
        await page.Locator("#overlay_file_menu").GetByText("Copy").ClickAsync();

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
