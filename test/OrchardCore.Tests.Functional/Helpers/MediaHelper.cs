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
    /// Navigate to the Media admin page.
    /// </summary>
    public static async Task NavigateToMediaAsync(IPage page, string prefix = "")
    {
        await page.GotoAsync($"{prefix}/Admin/Media");
        await page.Locator("text=Media Library").First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 30_000 });
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
            await page.Locator("text=Media Library").First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15_000 });
            await Assertions.Expect(locator).ToBeVisibleAsync(new() { Timeout = timeoutMs / 2 });
        }
    }
}
