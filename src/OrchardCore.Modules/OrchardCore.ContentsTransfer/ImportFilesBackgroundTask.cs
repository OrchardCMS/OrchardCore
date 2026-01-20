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

        try
        {
            using var spreadsheetDocument = SpreadsheetDocument.Open(stream, false);
            var workbookPart = spreadsheetDocument.WorkbookPart;

            if (workbookPart == null)
            {
                error(string.Empty, S["Unable to read the uploaded file."]);
                return dataTable;
            }

            var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

            if (sheets.Count == 0)
            {
                error(string.Empty, S["Unable to find a tab in the file that contains data."]);
                return dataTable;
            }

            var sheet = sheets[0];
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            if (sheetData == null)
            {
                error(string.Empty, S["Unable to find a tab in the file that contains data."]);
                return dataTable;
            }

            var rows = sheetData.Descendants<Row>().ToList();

            if (rows.Count == 0)
            {
                error(string.Empty, S["Unable to find a tab in the file that contains data."]);
                return dataTable;
            }

            // Get shared string table for reading string values
            var sharedStringTable = workbookPart.GetPartsOfType<SharedStringTablePart>()
                .FirstOrDefault()?.SharedStringTable;

            // Create a list to hold the column names.
            var columnNames = new List<string>();
            var columnMapping = new Dictionary<string, int>();

            // Process header row
            var headerRow = rows[0];
            var headerCells = headerRow.Descendants<Cell>().ToList();

            foreach (var cell in headerCells)
            {
                var columnName = GetCellValue(cell, sharedStringTable)?.Trim() ?? string.Empty;
                var columnIndex = GetColumnIndexFromCellReference(cell.CellReference);

                // Handle empty column names
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Col " + (columnIndex + 1);
                }

                // Add the column name to the list to count the duplicates.
                columnNames.Add(columnName);

                // Count the duplicate column names and make them unique
                var occurrences = columnNames.Count(x => x.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                if (occurrences > 1)
                {
                    columnName += " " + occurrences;
                }

                var column = new DataColumn(columnName);
                dataTable.Columns.Add(column);
                columnMapping[cell.CellReference.Value] = dataTable.Columns.Count - 1;
            }

            // Process data rows (skip header)
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var newRow = dataTable.NewRow();

                foreach (var cell in row.Descendants<Cell>())
                {
                    var columnIndex = GetColumnIndexFromCellReference(cell.CellReference);

                    if (columnIndex < dataTable.Columns.Count)
                    {
                        try
                        {
                            newRow[columnIndex] = GetCellValue(cell, sharedStringTable);
                        }
                        catch
                        {
                            // Ignore cell parsing errors
                        }
                    }
                }

                dataTable.Rows.Add(newRow);
            }
        }
        catch (Exception ex)
        {
            error(string.Empty, S["Unable to read the uploaded file: {0}", ex.Message]);
        }

        return dataTable;
    }

    private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell == null || cell.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.Text;

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            if (sharedStringTable != null && int.TryParse(value, out var index))
            {
                return sharedStringTable.ElementAt(index).InnerText;
            }
        }

        return value ?? string.Empty;
    }

    private static int GetColumnIndexFromCellReference(string cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return 0;
        }

        var columnLetters = string.Empty;
        foreach (var c in cellReference)
        {
            if (char.IsLetter(c))
            {
                columnLetters += c;
            }
            else
            {
                break;
            }
        }

        var columnIndex = 0;
        var multiplier = 1;

        for (var i = columnLetters.Length - 1; i >= 0; i--)
        {
            columnIndex += (columnLetters[i] - 'A' + 1) * multiplier;
            multiplier *= 26;
        }

        return columnIndex - 1; // Zero-based index
    }
}
