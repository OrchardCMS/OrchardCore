using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Exports pipeline data to an Excel workbook.
/// </summary>
public sealed class ExcelExportLoad : EtlLoadActivity
{
    public override string Name => nameof(ExcelExportLoad);

    public override string DisplayText => "Excel Workbook Export";

    public string FileName
    {
        get => GetProperty(() => "etl-export.xlsx");
        set => SetProperty(value);
    }

    public string WorksheetName
    {
        get => GetProperty(() => "Data");
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done"), new EtlOutcome("Failed")];
    }

    public override async Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        var data = context.DataStream;
        if (data == null)
        {
            return EtlActivityResult.Failure("No data stream available.");
        }

        var logger = context.ServiceProvider.GetRequiredService<ILogger<ExcelExportLoad>>();
        var mediaFileStore = context.ServiceProvider.GetService<IMediaFileStore>();

        if (mediaFileStore is null)
        {
            return EtlActivityResult.Failure("No IMediaFileStore service is available for Excel export.");
        }

        try
        {
            var rows = new List<JsonObject>();
            var headers = new List<string>();

            await foreach (var record in data.WithCancellation(context.CancellationToken))
            {
                rows.Add(record.DeepClone().AsObject());

                foreach (var property in record)
                {
                    if (!headers.Contains(property.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        headers.Add(property.Key);
                    }
                }
            }

            await using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = WorksheetName,
                });

                var headerRow = new Row();
                foreach (var header in headers)
                {
                    headerRow.Append(CreateTextCell(header));
                }

                sheetData.Append(headerRow);

                foreach (var row in rows)
                {
                    var sheetRow = new Row();
                    foreach (var header in headers)
                    {
                        sheetRow.Append(CreateTextCell(row[header]?.ToString() ?? string.Empty));
                    }

                    sheetData.Append(sheetRow);
                }

                workbookPart.Workbook.Save();
            }

            stream.Position = 0;
            var fileName = mediaFileStore.NormalizePath(FileName);
            await mediaFileStore.CreateFileFromStreamAsync(fileName, stream, overwrite: true);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("ETL Excel export wrote {Count} records to '{FileName}'.", rows.Count, fileName);
            }

            return Outcomes("Done");
        }
        catch (Exception ex)
        {
            var fileName = FileName;

            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "ETL Excel export failed for '{FileName}'.", fileName);
            }

            return EtlActivityResult.Failure($"Excel export failed for '{fileName}': {ex.Message}");
        }
    }

    private static Cell CreateTextCell(string value)
    {
        return new Cell
        {
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(value ?? string.Empty)),
        };
    }
}
