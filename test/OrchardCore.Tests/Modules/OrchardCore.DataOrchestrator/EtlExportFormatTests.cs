using System.Text.Json.Nodes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class EtlExportFormatTests
{
    [Fact]
    public async Task JsonExportFormat_WritesArrayOfRecords()
    {
        var records = new List<JsonObject>
        {
            new() { ["name"] = "a", ["value"] = 1 },
            new() { ["name"] = "b", ["value"] = 2 },
        };

        var format = new JsonExportFormat();

        using var stream = new MemoryStream();
        await format.WriteAsync(records, stream, CancellationToken.None);
        stream.Position = 0;

        var parsed = JsonNode.Parse(stream) as JsonArray;

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Count);
        Assert.Equal("a", parsed[0]["name"].GetValue<string>());
        Assert.Equal(2, parsed[1]["value"].GetValue<int>());
        Assert.Equal("json", format.FileExtension);
    }

    [Fact]
    public async Task CsvExportFormat_WritesHeaderUnionAndEscapesValues()
    {
        var records = new List<JsonObject>
        {
            new() { ["name"] = "a,b", ["note"] = "he said \"hi\"" },
            new() { ["name"] = "c" },
        };

        var format = new CsvExportFormat();

        using var stream = new MemoryStream();
        await format.WriteAsync(records, stream, CancellationToken.None);

        var text = Encoding.UTF8.GetString(stream.ToArray());
        var lines = text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        Assert.Equal("name,note", lines[0]);
        Assert.Equal("\"a,b\",\"he said \"\"hi\"\"\"", lines[1]);
        Assert.Equal("c,", lines[2]);
    }

    [Fact]
    public async Task ExcelExportFormat_ProducesReadableWorkbook()
    {
        var records = new List<JsonObject>
        {
            new() { ["name"] = "a", ["value"] = "1" },
            new() { ["name"] = "b", ["value"] = "2" },
        };

        var format = new ExcelExportFormat();

        using var stream = new MemoryStream();
        await format.WriteAsync(records, stream, CancellationToken.None);

        Assert.True(stream.Length > 0);

        stream.Position = 0;
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart;

        Assert.NotNull(workbookPart);

        var worksheetPart = workbookPart.WorksheetParts.First();
        var rows = worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>().ToList();

        // Header row + 2 data rows.
        Assert.Equal(3, rows.Count);

        var headerCells = rows[0].Elements<Cell>().Select(c => c.InnerText).ToList();
        Assert.Contains("name", headerCells);
        Assert.Contains("value", headerCells);
    }

    [Fact]
    public void ExportFormatProvider_ResolvesByNameCaseInsensitively()
    {
        var provider = new EtlExportFormatProvider(
        [
            new JsonExportFormat(),
            new CsvExportFormat(),
            new ExcelExportFormat(),
        ]);

        Assert.Equal("Json", provider.GetFormat("json").Name);
        Assert.Equal("Csv", provider.GetFormat("CSV").Name);
        Assert.Null(provider.GetFormat("unknown"));
        Assert.Null(provider.GetFormat(null));
        Assert.Equal(3, provider.ListFormats().Count());
    }
}
