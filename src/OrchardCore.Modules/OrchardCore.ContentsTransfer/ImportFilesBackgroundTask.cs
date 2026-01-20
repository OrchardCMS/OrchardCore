using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ContentsTransfer;

[BackgroundTask(
    Title = "Imported Files Processor",
    Schedule = "*/10 * * * *",
    Description = "Regularly check for imported files and process them.",
    LockTimeout = 3_000, LockExpiration = 30_000)]
public class ImportFilesBackgroundTask : IBackgroundTask
{
    private ISession _session;
    private IClock _clock;
    private ILogger _logger;
    private IContentManager _contentManager;
    protected IStringLocalizer S;

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _session = serviceProvider.GetRequiredService<ISession>();
        _clock = serviceProvider.GetRequiredService<IClock>();
        _logger = serviceProvider.GetRequiredService<ILogger<ImportFilesBackgroundTask>>();
        _contentManager = serviceProvider.GetRequiredService<IContentManager>();
        var contentImportOptions = serviceProvider.GetRequiredService<IOptions<ContentImportOptions>>().Value;
        S = serviceProvider.GetRequiredService<IStringLocalizer<ImportFilesBackgroundTask>>();

        var fileStore = serviceProvider.GetRequiredService<IContentTransferFileStore>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentImportManager = serviceProvider.GetRequiredService<IContentImportManager>();

        var entries = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.Status == ContentTransferEntryStatus.New || x.Status == ContentTransferEntryStatus.Processing)
        .OrderBy(x => x.CreatedUtc)
        .ListAsync();

        var batchSize = contentImportOptions.ImportBatchSize < 1 ? 100 : contentImportOptions.ImportBatchSize;

        foreach (var entry in entries)
        {
            var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(entry.ContentType);

            if (contentTypeDefinition == null)
            {
                // The content type was removed somehow. Skip it.
                await SaveEntryWithError(entry, S["The content definition was removed."]);

                continue;
            }

            var fileInfo = await fileStore.GetFileInfoAsync(entry.StoredFileName);

            if (fileInfo.Length == 0)
            {
                // The file was removed somehow. Skip it.
                await SaveEntryWithError(entry, S["The import file no longer exists."]);

                continue;
            }

            var file = await fileStore.GetFileStreamAsync(fileInfo);
            var errors = new Dictionary<string, string>();
            using var dataTable = GetDataTable(file, errors.Add);
            await file.DisposeAsync();

            if (errors.Count > 0)
            {
                // The file was removed somehow. Skip it.
                await SaveEntryWithError(entry, errors.First().Value);

                continue;
            }

            entry.Status = ContentTransferEntryStatus.Processing;

            var progressPart = entry.As<ImportFileProcessStatsPart>();

            progressPart.TotalRecords = dataTable.Rows.Count;
            progressPart.Errors ??= [];
            var contentItems = new Dictionary<int, ContentItem>();
            var records = new Dictionary<int, DataRow>();
            var existingRows = new Dictionary<string, KeyValuePair<int, DataRow>>(StringComparer.OrdinalIgnoreCase);
            var indexOfKeyColumn = dataTable.Columns.IndexOf(nameof(ContentItem.ContentItemId));

            foreach (DataRow row in dataTable.Rows)
            {
                var rowIndex = dataTable.Rows.IndexOf(row);

                if (rowIndex > 0 && rowIndex <= progressPart.CurrentRow)
                {
                    // At this point, we know that this record was previously processed. Skip it.
                    continue;
                }

                progressPart.TotalProcessed++;
                progressPart.CurrentRow = rowIndex;

                if (row == null || row.ItemArray.All(x => x == null || x is DBNull || string.IsNullOrWhiteSpace(x?.ToString())))
                {
                    // Ignore empty rows.
                    continue;
                }

                if (indexOfKeyColumn > -1)
                {
                    var contentItemId = row[indexOfKeyColumn]?.ToString()?.Trim();

                    if (!string.IsNullOrEmpty(contentItemId))
                    {
                        existingRows.TryAdd(contentItemId, new KeyValuePair<int, DataRow>(rowIndex, row));
                    }
                    else
                    {
                        records.Add(rowIndex, row);
                    }
                }
                else
                {
                    records.Add(rowIndex, row);
                }

                if (records.Count + existingRows.Count == batchSize)
                {
                    if (existingRows.Count > 0)
                    {
                        var existingContentItems = (await _contentManager.GetAsync(existingRows.Keys, VersionOptions.DraftRequired)).ToDictionary(x => x.ContentItemId);

                        foreach (var existingRow in existingRows)
                        {
                            ContentItem contentItem;
                            var isNew = false;
                            if (!existingContentItems.TryGetValue(existingRow.Key, out contentItem))
                            {
                                contentItem = await _contentManager.NewAsync(entry.ContentType);
                                isNew = true;
                            }

                            var mapContext = new ContentImportContext()
                            {
                                ContentItem = contentItem,
                                ContentTypeDefinition = contentTypeDefinition,
                                Columns = dataTable.Columns,
                                Row = existingRow.Value.Value,
                            };

                            await contentImportManager.ImportAsync(mapContext);

                            var validationResult = await _contentManager.ValidateAsync(mapContext.ContentItem);

                            if (validationResult.Succeeded)
                            {
                                if (isNew)
                                {
                                    await _contentManager.CreateAsync(mapContext.ContentItem, VersionOptions.DraftRequired);

                                    mapContext.ContentItem.Owner = entry.Owner;
                                    mapContext.ContentItem.Author = entry.Author;
                                }
                                else
                                {
                                    await _contentManager.UpdateAsync(mapContext.ContentItem);
                                    mapContext.ContentItem.Author = entry.Author;
                                }

                                await _contentManager.PublishAsync(mapContext.ContentItem);
                            }
                            else
                            {
                                progressPart.Errors.Add(existingRow.Value.Key);
                            }
                        }
                    }

                    foreach (var record in records)
                    {
                        var mapContext = new ContentImportContext()
                        {
                            ContentItem = await _contentManager.NewAsync(entry.ContentType),
                            ContentTypeDefinition = contentTypeDefinition,
                            Columns = dataTable.Columns,
                            Row = record.Value,
                        };

                        await contentImportManager.ImportAsync(mapContext);

                        var validationResult = await _contentManager.ValidateAsync(mapContext.ContentItem);

                        if (validationResult.Succeeded)
                        {
                            await _contentManager.CreateAsync(mapContext.ContentItem, VersionOptions.DraftRequired);

                            mapContext.ContentItem.Owner = entry.Owner;
                            mapContext.ContentItem.Author = entry.Author;

                            await _contentManager.PublishAsync(mapContext.ContentItem);
                        }
                        else
                        {
                            progressPart.Errors.Add(record.Key);
                        }
                    }

                    entry.ProcessSaveUtc = _clock.UtcNow;
                    entry.Put(progressPart);

                    _session.Save(entry);
                    await _session.SaveChangesAsync();
                }
            }

            var nowUtc = _clock.UtcNow;
            entry.ProcessSaveUtc = nowUtc;
            entry.CompletedUtc = nowUtc;
            entry.Status = progressPart.Errors?.Count > 0 ? ContentTransferEntryStatus.CompletedWithErrors : ContentTransferEntryStatus.Completed;
            entry.Put(progressPart);

            _session.Save(entry);
            await _session.SaveChangesAsync();
        }
    }

    private async Task SaveEntryWithError(ContentTransferEntry entry, string error)
    {
        entry.Status = ContentTransferEntryStatus.Failed;
        entry.Error = error;
        entry.CompletedUtc = _clock.UtcNow;

        _session.Save(entry);

        await _session.SaveChangesAsync();
    }

    private DataTable GetDataTable(Stream stream, Action<string, string> error)
    {
        var dataTable = new DataTable();

        // Copy stream to MemoryStream since SpreadsheetDocument requires a seekable stream
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart == null)
        {
            error(string.Empty, S["Unable to read the workbook from the uploaded file."]);
            return dataTable;
        }

        var sheets = workbookPart.Workbook.Sheets;
        if (sheets == null || !sheets.Elements<Sheet>().Any())
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);
            return dataTable;
        }

        var sheet = sheets.Elements<Sheet>().FirstOrDefault();
        if (sheet?.Id?.Value == null)
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);
            return dataTable;
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

        if (sheetData == null || !sheetData.Elements<Row>().Any())
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);
            return dataTable;
        }

        // Get shared strings for cell value resolution
        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

        // Get all rows
        var rows = sheetData.Elements<Row>().ToList();

        if (rows.Count == 0)
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);
            return dataTable;
        }

        // Process header row (first row)
        var headerRow = rows[0];
        var columnNames = new List<string>();
        var maxColumnIndex = 0;

        foreach (var cell in headerRow.Elements<Cell>())
        {
            var columnIndex = GetColumnIndex(cell.CellReference?.Value);
            if (columnIndex > maxColumnIndex)
            {
                maxColumnIndex = columnIndex;
            }
        }

        // Add columns to dataTable
        for (var i = 0; i < maxColumnIndex; i++)
        {
            var columnName = $"Col{i + 1}";
            columnNames.Add(columnName);
            dataTable.Columns.Add(columnName);
        }

        // Now update column names from header cells
        foreach (var cell in headerRow.Elements<Cell>())
        {
            var columnIndex = GetColumnIndex(cell.CellReference?.Value) - 1;
            if (columnIndex >= 0 && columnIndex < dataTable.Columns.Count)
            {
                var cellValue = GetCellValue(cell, sharedStringTable)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(cellValue))
                {
                    // Count duplicates and make unique
                    var occurrences = columnNames.Take(columnIndex + 1).Count(x => x.Equals(cellValue, StringComparison.OrdinalIgnoreCase));
                    if (occurrences > 0)
                    {
                        var existingIndex = columnNames.IndexOf(cellValue);
                        if (existingIndex >= 0 && existingIndex < columnIndex)
                        {
                            cellValue += " " + (occurrences + 1);
                        }
                    }
                    columnNames[columnIndex] = cellValue;
                    dataTable.Columns[columnIndex].ColumnName = cellValue;
                }
            }
        }

        // Process data rows (skip header row)
        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var newRow = dataTable.NewRow();

            foreach (var cell in row.Elements<Cell>())
            {
                var columnIndex = GetColumnIndex(cell.CellReference?.Value) - 1;
                if (columnIndex >= 0 && columnIndex < dataTable.Columns.Count)
                {
                    var cellValue = GetCellValue(cell, sharedStringTable);
                    newRow[columnIndex] = cellValue ?? string.Empty;
                }
            }

            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }

    private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell?.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.Text;

        if (cell.DataType?.Value == CellValues.SharedString && sharedStringTable != null)
        {
            if (int.TryParse(value, out var sharedStringIndex))
            {
                var sharedStringItem = sharedStringTable.Elements<SharedStringItem>().ElementAtOrDefault(sharedStringIndex);
                return sharedStringItem?.InnerText ?? string.Empty;
            }
        }

        return value ?? string.Empty;
    }

    private static int GetColumnIndex(string cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return 0;
        }

        var columnPart = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var columnIndex = 0;

        foreach (var c in columnPart.ToUpperInvariant())
        {
            columnIndex = columnIndex * 26 + (c - 'A' + 1);
        }

        return columnIndex;
    }
}
