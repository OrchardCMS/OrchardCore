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
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentTransfer;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ContentsTransfer;

[BackgroundTask(
    Title = "Import Files Processor",
    Schedule = "*/10 * * * *",
    Description = "Regularly check for imported files and process them.",
    LockTimeout = 3_000, LockExpiration = 30_000)]
public class ImportFilesBackgroundTask : IBackgroundTask
{
    private static readonly JsonMergeSettings _updateJsonMergeSettings = new JsonMergeSettings
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
        MergeNullValueHandling = MergeNullValueHandling.Ignore,
    };

    private const int _batchSize = 100;

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
        S = serviceProvider.GetRequiredService<IStringLocalizer<ImportFilesBackgroundTask>>();

        var fileStore = serviceProvider.GetRequiredService<IContentTransferFileStore>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentImportManager = serviceProvider.GetRequiredService<IContentImportManager>();

        var entries = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.Status == ContentTransferEntryStatus.New || x.Status == ContentTransferEntryStatus.Processing)
        .OrderBy(x => x.CreatedUtc)
        .ListAsync();

        foreach (var entry in entries)
        {
            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(entry.ContentType);

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
            using var dataTable = GetDataTable(errors.Add);

            if (errors.Count > 0)
            {
                // The file was removed somehow. Skip it.
                await SaveEntryWithError(entry, errors.First().Value);

                continue;
            }

            var progressPart = entry.As<ImportFileProcessStatsPart>();

            var contentItems = new List<ContentItem>();

            foreach (DataRow row in dataTable.Rows)
            {
                if (row == null || row.ItemArray.All(x => x == null || x is DBNull || string.IsNullOrWhiteSpace(x?.ToString())))
                {
                    // Ignore empty rows.
                    progressPart.TotalProcessed++;

                    continue;
                }

                var mapContext = new ContentImportContext()
                {
                    ContentItem = await _contentManager.NewAsync(entry.ContentType),
                    ContentTypeDefinition = contentTypeDefinition,
                    Columns = dataTable.Columns,
                    Row = row,
                };

                // Important to import the data first as it could identify existing content item.
                // ImportAsync could change the content item id.
                await contentImportManager.ImportAsync(mapContext);

                contentItems.Insert(dataTable.Rows.IndexOf(row), mapContext.ContentItem);

                if (contentItems.Count == _batchSize)
                {
                    await SavePatchAsync(contentItems, progressPart);
                    entry.ProcessSaveUtc = _clock.UtcNow;
                    entry.Put(progressPart);

                    _session.Save(entry);
                    await _session.SaveChangesAsync();

                    contentItems.Clear();
                }
            }

            await SavePatchAsync(contentItems, progressPart);
            var nowUtc = _clock.UtcNow;
            entry.ProcessSaveUtc = nowUtc;
            entry.CompletedUtc = nowUtc;
            entry.Status = progressPart.Errors.Count > 0 ? ContentTransferEntryStatus.CompletedWithErrors : ContentTransferEntryStatus.Completed;
            entry.Put(progressPart);

            _session.Save(entry);
            await _session.SaveChangesAsync();

            contentItems.Clear();
        }
    }

    private async Task SavePatchAsync(IList<ContentItem> contentItems, ImportFileProcessStatsPart stats)
    {
        var existingItemIds = contentItems.Where(x => !string.IsNullOrEmpty(x.ContentItemId)).Select(x => x.ContentItemId).ToList();

        // Attempt to find existing content item.
        var existingContentItems = (await _contentManager.GetAsync(existingItemIds, VersionOptions.Latest)).ToDictionary(x => x.ContentItemId, x => x);

        foreach (var contentItem in contentItems)
        {
            if (existingContentItems.TryGetValue(contentItem.ContentItemId, out var existingContentItem))
            {
                // At this point, we are updating existing content item.
                // Merge new values to existing.
                existingContentItem.Merge(contentItem, _updateJsonMergeSettings);

                if (!await TrySaveRecordAsync(existingContentItem))
                {
                    stats.Errors.Add(contentItems.IndexOf(contentItem));
                }

                continue;
            }

            // At this point, we are creating a new content item.
            if (!await TrySaveRecordAsync(contentItem))
            {
                stats.Errors.Add(contentItems.IndexOf(contentItem));
            }
        }
    }

    private async Task<bool> TrySaveRecordAsync(ContentItem existingContentItem)
    {
        var result = await _contentManager.ValidateAsync(existingContentItem);

        if (result.Succeeded)
        {
            _session.Save(existingContentItem);

            return true;
        }

        return false;
    }

    private async Task SaveEntryWithError(ContentTransferEntry entry, string error)
    {
        entry.Status = ContentTransferEntryStatus.Failed;
        entry.Error = error;
        entry.CompletedUtc = _clock.UtcNow;

        _session.Save(entry);

        await _session.SaveChangesAsync();
    }

    private DataTable GetDataTable(Action<string, string> error)
    {
        using var stream = new MemoryStream();
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
