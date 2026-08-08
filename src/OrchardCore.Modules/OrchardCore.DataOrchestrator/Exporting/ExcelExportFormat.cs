using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OrchardCore.DataOrchestrator.Exporting;

/// <summary>
/// Serializes records as an Excel (<c>.xlsx</c>) workbook. The header row is built from the
/// union of every record's top-level property names.
/// </summary>
public sealed class ExcelExportFormat : IEtlExportFormat
{
    public string Name => "Excel";

    public string DisplayText => "Excel Workbook";

    public string FileExtension => "xlsx";

    public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task WriteAsync(IAsyncEnumerable<JsonObject> records, Stream output, CancellationToken cancellationToken)
    {
        var rows = new List<JsonObject>();
        var headers = new List<string>();
        var headerSet = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var record in records.WithCancellation(cancellationToken))
        {
            rows.Add(record);

            foreach (var property in record)
            {
                if (headerSet.Add(property.Key))
                {
                    headers.Add(property.Key);
                }
            }
        }

        using (var document = SpreadsheetDocument.Create(output, SpreadsheetDocumentType.Workbook, true))
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
                Name = "Data",
            });

            var headerRow = new Row();

            foreach (var header in headers)
            {
                headerRow.Append(CreateTextCell(header));
            }

            sheetData.Append(headerRow);

            foreach (var record in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var sheetRow = new Row();

                foreach (var header in headers)
                {
                    var value = record.TryGetPropertyValue(header, out var node) && node is not null
                        ? node.ToString()
                        : string.Empty;

                    sheetRow.Append(CreateTextCell(value));
                }

                sheetData.Append(sheetRow);
            }

            workbookPart.Workbook.Save();
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
