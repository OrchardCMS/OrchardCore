using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Extracts rows from an Excel workbook stored in the media library.
/// </summary>
public sealed class ExcelSource : EtlSourceActivity
{
    public override string Name => nameof(ExcelSource);

    public override string DisplayText => "Excel Workbook";

    public string FilePath
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string WorksheetName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public bool HasHeaderRow
    {
        get => GetProperty(() => true);
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done")];
    }

    public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        context.DataStream = ExtractAsync(context.ServiceProvider, FilePath, WorksheetName, HasHeaderRow, context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }

    private static async IAsyncEnumerable<JsonObject> ExtractAsync(
        IServiceProvider serviceProvider,
        string filePath,
        string worksheetName,
        bool hasHeaderRow,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            yield break;
        }

        var mediaFileStore = serviceProvider.GetRequiredService<IMediaFileStore>();
        var normalizedPath = mediaFileStore.NormalizePath(filePath);
        var fileInfo = await mediaFileStore.GetFileInfoAsync(normalizedPath);

        if (fileInfo == null)
        {
            yield break;
        }

        await using var stream = await mediaFileStore.GetFileStreamAsync(fileInfo);
        using var spreadsheetDocument = SpreadsheetDocument.Open(stream, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart?.Workbook == null)
        {
            yield break;
        }

        var sharedStringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()?.SharedStringTable;
        var sheet = string.IsNullOrWhiteSpace(worksheetName)
            ? workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault()
            : workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(x => string.Equals(x.Name?.Value, worksheetName, StringComparison.OrdinalIgnoreCase));

        if (sheet == null)
        {
            yield break;
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

        if (sheetData == null)
        {
            yield break;
        }

        var rows = sheetData.Elements<Row>().ToList();

        if (rows.Count == 0)
        {
            yield break;
        }

        var headerValues = GetRowValues(rows[0], sharedStringTable);
        var columnNames = BuildColumnNames(headerValues, hasHeaderRow);
        var dataRows = hasHeaderRow ? rows.Skip(1) : rows;

        foreach (var row in dataRows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var values = GetRowValues(row, sharedStringTable);
            var record = new JsonObject();

            for (var i = 0; i < columnNames.Count; i++)
            {
                record[columnNames[i]] = i < values.Count ? values[i] : string.Empty;
            }

            yield return record;
        }
    }

    private static List<string> BuildColumnNames(IReadOnlyList<string> headerValues, bool hasHeaderRow)
    {
        if (!hasHeaderRow)
        {
            return Enumerable.Range(1, headerValues.Count == 0 ? 1 : headerValues.Count)
                .Select(index => $"Column{index}")
                .ToList();
        }

        var columnNames = new List<string>();

        for (var i = 0; i < headerValues.Count; i++)
        {
            var header = string.IsNullOrWhiteSpace(headerValues[i]) ? $"Column{i + 1}" : headerValues[i].Trim();
            var uniqueHeader = header;
            var suffix = 2;

            while (columnNames.Contains(uniqueHeader, StringComparer.OrdinalIgnoreCase))
            {
                uniqueHeader = $"{header}_{suffix++}";
            }

            columnNames.Add(uniqueHeader);
        }

        return columnNames.Count == 0 ? ["Column1"] : columnNames;
    }

    private static List<string> GetRowValues(Row row, SharedStringTable sharedStringTable)
    {
        var values = new List<string>();

        foreach (var cell in row.Elements<Cell>())
        {
            var columnIndex = GetColumnIndexFromCellReference(cell.CellReference);

            while (values.Count <= columnIndex)
            {
                values.Add(string.Empty);
            }

            values[columnIndex] = GetCellValue(cell, sharedStringTable);
        }

        return values;
    }

    private static int GetColumnIndexFromCellReference(StringValue cellReference)
    {
        var reference = cellReference?.Value ?? string.Empty;
        var columnName = new string(reference.TakeWhile(char.IsLetter).ToArray());
        var columnIndex = 0;

        foreach (var ch in columnName)
        {
            columnIndex *= 26;
            columnIndex += ch - 'A' + 1;
        }

        return Math.Max(columnIndex - 1, 0);
    }

    private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.InnerText;

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(value, out var sharedStringIndex) &&
            sharedStringTable?.ElementAtOrDefault(sharedStringIndex) is SharedStringItem sharedStringItem)
        {
            return sharedStringItem.InnerText;
        }

        return value;
    }
}
