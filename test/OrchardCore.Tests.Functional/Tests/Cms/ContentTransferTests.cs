using System.Text;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class ContentTransferTests : CmsTestBase<ContentTransferFixture>, IClassFixture<ContentTransferFixture>
{
    public ContentTransferTests(ContentTransferFixture fixture) : base(fixture) { }

    private static ILocator UploadButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Upload" });

    private static ILocator ExportButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Export Data" });

    [Fact]
    public async Task ImportPageIsAccessible()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");
            await Assertions.Expect(page.Locator("#import-form")).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ImportPageShowsFileRequirements()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");
            await Assertions.Expect(page.GetByText(".xlsx (Excel) or .csv format")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByText("Download Template (Excel)")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByText("Download Template (CSV)")).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanDownloadExcelTemplate()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");

            var downloadTask = page.WaitForDownloadAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Download Template (Excel)" }).ClickAsync();
            var download = await downloadTask;

            Assert.Contains("TestArticle_Template.xlsx", download.SuggestedFilename);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanDownloadCsvTemplate()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");

            var downloadTask = page.WaitForDownloadAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Download Template (CSV)" }).ClickAsync();
            var download = await downloadTask;

            Assert.Contains("TestArticle_Template.csv", download.SuggestedFilename);

            // Verify the CSV content has a header row.
            await using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var header = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(header);
            Assert.Contains("Title", header);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanUploadCsvFileForImport()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");

            var csvContent = "Title,TestArticle.Description.Text\nCSV Import Test,A description from CSV";
            var fileInput = page.Locator("input[type='file']");
            await fileInput.SetInputFilesAsync(new FilePayload
            {
                Name = "test-import.csv",
                MimeType = "text/csv",
                Buffer = Encoding.UTF8.GetBytes(csvContent),
            });

            // Submit the form - this triggers the chunked upload JS.
            await UploadButton(page).ClickAsync();

            // Wait for redirect to the list page (indicates success).
            await page.WaitForURLAsync("**/content-transfer-entries**", new() { Timeout = 30000 });
            Assert.Contains("content-transfer-entries", page.Url);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanUploadXlsxFileForImport()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();

            // First download a template to get a valid xlsx file.
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");
            var downloadTask = page.WaitForDownloadAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Download Template (Excel)" }).ClickAsync();
            var download = await downloadTask;

            // Use the template as an import file.
            var tempPath = Path.GetTempFileName() + ".xlsx";
            await download.SaveAsAsync(tempPath);

            try
            {
                await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");
                var fileInput = page.Locator("input[type='file']");
                await fileInput.SetInputFilesAsync(tempPath);

                await UploadButton(page).ClickAsync();

                // Wait for redirect to the list page (indicates success).
                await page.WaitForURLAsync("**/content-transfer-entries**", new() { Timeout = 30000 });
                Assert.Contains("content-transfer-entries", page.Url);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ExportPageIsAccessible()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/export/contents");
            Assert.Equal(1, await page.Locator("#Extension option[value='.csv']").CountAsync());
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanExportContentAsExcel()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();

            var downloadTask = page.WaitForDownloadAsync();
            await page.EvaluateAsync(
                "() => { window.location.href = '/Admin/export/contents/download-file?contentTypeId=TestArticle&format=xlsx'; }");
            var download = await downloadTask;

            Assert.Contains("TestArticle_Export.xlsx", download.SuggestedFilename);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CanExportContentAsCsv()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();

            var downloadTask = page.WaitForDownloadAsync();
            await page.EvaluateAsync(
                "() => { window.location.href = '/Admin/export/contents/download-file?contentTypeId=TestArticle&format=csv'; }");
            var download = await downloadTask;

            Assert.Contains("TestArticle_Export.csv", download.SuggestedFilename);

            // Verify it's valid CSV.
            await using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var header = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(header);
            Assert.Contains("Title", header);

            // Should have at least one exported data row.
            var dataLine = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(dataLine);
            Assert.NotEmpty(dataLine);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task BulkImportListShowsEntries()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/content-transfer-entries");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task RejectsUnsupportedFileFormat()
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync("/Admin/import/contents/TestArticle");

            var fileInput = page.Locator("input[type='file']");
            await fileInput.SetInputFilesAsync(new FilePayload
            {
                Name = "test.txt",
                MimeType = "text/plain",
                Buffer = Encoding.UTF8.GetBytes("Title\nTest"),
            });

            // Submit the form.
            await UploadButton(page).ClickAsync();

            // Should show an error message (the upload stays on the same page).
            await Assertions.Expect(page.Locator("#upload-error")).ToBeVisibleAsync(new() { Timeout = 10000 });
            var errorText = await page.Locator("#upload-error").TextContentAsync();
            Assert.Contains("supported", errorText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
