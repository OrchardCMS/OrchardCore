using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
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
        using var package = new ExcelPackage(stream);
        using var workbook = package.Workbook;
        using var worksheets = workbook.Worksheets;
        var dataTable = new DataTable();

        if (worksheets.Count == 0)
        {
            error(string.Empty, S["More than one tab are found on the uploaded file. Please delete any additional tabs and re-upload the file."]);

            return dataTable;
        }

        using var worksheet = worksheets[0];

        // Check if the worksheet is completely empty.
        if (worksheet.Dimension == null)
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);

            return dataTable;
        }

        // Create a list to hold the column names.
        var columnNames = new List<string>();

        // Needed to keep track of empty column headers.
        var currentColumn = 1;

        foreach (var cell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
        {
            var columnName = cell.Text.Trim();

            // Check if the previous header was empty, create a header name.
            if (cell.Start.Column != currentColumn)
            {
                columnNames.Add("Col " + currentColumn);
                dataTable.Columns.Add("Col " + currentColumn);
                currentColumn++;
            }

            // Add the column name to the list to count the duplicates.
            columnNames.Add(columnName);

            // Count the duplicate column names and make them unique to avoid the exception
            // A column named 'Name' already belongs to this DataTable
            var occurrences = columnNames.Count(x => x.Equals(columnName));
            if (occurrences > 1)
            {
                columnName += " " + occurrences;
            }

            var column = new DataColumn(columnName);

            // Add the column to the dataTable
            dataTable.Columns.Add(column);

            currentColumn++;
        }

        // Start adding the contents of the excel file to the dataTable
        for (var i = 2; i <= worksheet.Dimension.End.Row; i++)
        {
            var row = worksheet.Cells[i, 1, i, worksheet.Dimension.End.Column];
            var newRow = dataTable.NewRow();

            foreach (var cell in row)
            {
                try
                {
                    newRow[cell.Start.Column - 1] = cell.Text;
                }
                catch { }
            }

            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }
}
