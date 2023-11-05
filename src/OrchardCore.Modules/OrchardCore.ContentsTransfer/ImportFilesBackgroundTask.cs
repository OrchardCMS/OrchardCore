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
    private const int _batchSize = 100;

    private ISession _session;
    private IClock _clock;
    private ILogger _logger;
    protected IStringLocalizer S;

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _session = serviceProvider.GetRequiredService<ISession>();
        _clock = serviceProvider.GetRequiredService<IClock>();
        _logger = serviceProvider.GetRequiredService<ILogger<ImportFilesBackgroundTask>>();
        S = serviceProvider.GetRequiredService<IStringLocalizer<ImportFilesBackgroundTask>>();

        var fileStore = serviceProvider.GetRequiredService<IContentTransferFileStore>();
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var coordinator = serviceProvider.GetRequiredService<IEnumerable<IContentImportManager>>();

        var entries = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.Status == ContentTransferEntryStatus.New || x.Status == ContentTransferEntryStatus.Processing)
        .OrderBy(x => x.CreatedUtc)
        .ListAsync();

        foreach (var entry in entries)
        {
            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(entry.ContentType);

            if (contentTypeDefinition == null)
            {
                // The content type was removed somehow. Skip it.
                await SaveEntryWithError(entry);

                continue;
            }

            var fileInfo = await fileStore.GetFileInfoAsync(entry.StoredFileName);

            if (fileInfo.Length == 0)
            {
                // The file was removed somehow. Skip it.
                await SaveEntryWithError(entry);

                continue;
            }

            var file = await fileStore.GetFileStreamAsync(fileInfo);
            var errors = new Dictionary<string, string>();
            var dataTable = GetDataTable(errors.Add);

            if (errors.Count > 0)
            {
                // The file was removed somehow. Skip it.
                await SaveEntryWithError(entry);

                continue;
            }

            var progressPart = entry.As<ImportFileProcessStatsPart>();

            var contentItems = new List<ContentItem>();
            using var errorDataTable = new DataTable();
            foreach (DataRow row in dataTable.Rows)
            {
                if (row == null || row.ItemArray.All(x => x == null || x is DBNull || string.IsNullOrWhiteSpace(x?.ToString())))
                {
                    // Ignore empty rows.
                    continue;
                }

                var mapContext = new ContentImportMapContext()
                {
                    ContentItem = await contentManager.NewAsync(entry.ContentType),
                    ContentTypeDefinition = contentTypeDefinition,
                    Columns = dataTable.Columns,
                    Row = row,
                };

                // Important to map the data first since the map could identify existing content item.
                // MapAsync could change the content item id.
                await coordinator.InvokeAsync(mapper => mapper.ImportAsync(mapContext), _logger);
                var validationResult = await contentManager.ValidateAsync(mapContext.ContentItem);

                if (validationResult.Succeeded)
                {
                    contentItems.Add(mapContext.ContentItem);
                    progressPart.TotalSuccesses++;
                }
                else
                {
                    errorDataTable.Rows.Add(row);
                    progressPart.TotalErrors++;
                }

                if (contentItems.Count == _batchSize)
                {
                    entry.Put(progressPart);

                    _session.Save(entry);
                    await _session.SaveChangesAsync();

                    contentItems.Clear();
                    // save errorDataTable to an error file.
                }
            }

            entry.Put(progressPart);

            _session.Save(entry);
            await _session.SaveChangesAsync();
            // save errorDataTable to an error file.
            dataTable.Dispose();
            errorDataTable.Dispose();
        }
    }

    private async Task SaveEntryWithError(ContentTransferEntry entry)
    {
        entry.Status = ContentTransferEntryStatus.Failed;
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

        var worksheet = worksheets[0];

        // Check if the worksheet is completely empty.
        if (worksheet.Dimension == null)
        {
            error(string.Empty, S["Unable to find a tab in the file that contains data."]);
            worksheet.Dispose();
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

        worksheet.Dispose();

        return dataTable;
    }
}
