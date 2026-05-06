using System.Data;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.ContentTransfer.Models;
using OrchardCore.Entities;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ContentTransfer.BackgroundTasks;

[BackgroundTask(
    Title = "Export Files Processor",
    Schedule = "*/5 * * * *",
    Description = "Regularly check for queued export requests and process them.",
    LockTimeout = 3_000, LockExpiration = 30_000)]
public sealed class ExportFilesBackgroundTask : IBackgroundTask
{
    private static readonly TimeSpan _exportLockTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _exportLockExpiration = TimeSpan.FromMinutes(30);

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => ProcessEntriesAsync(serviceProvider, cancellationToken);

    internal static async Task ProcessEntriesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken, string entryId = null)
    {
        var session = serviceProvider.GetRequiredService<ISession>();
        var distributedLock = serviceProvider.GetRequiredService<IDistributedLock>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var contentImportOptions = serviceProvider.GetRequiredService<IOptions<ContentImportOptions>>().Value;
        var localizer = serviceProvider.GetRequiredService<IStringLocalizer<ExportFilesBackgroundTask>>();
        var fileStore = serviceProvider.GetRequiredService<IContentTransferFileStore>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentImportManager = serviceProvider.GetRequiredService<IContentImportManager>();

        var entries = await session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
            (x.Status == ContentTransferEntryStatus.New || x.Status == ContentTransferEntryStatus.Processing)
            && x.Direction == ContentTransferDirection.Export
            && (entryId == null || x.EntryId == entryId))
        .OrderBy(x => x.CreatedUtc)
        .ListAsync(cancellationToken);

        var batchSize = contentImportOptions.ExportBatchSize < 1
            ? ContentImportOptions.DefaultExportBatchSize
            : contentImportOptions.ExportBatchSize;

        foreach (var entry in entries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            (var locker, var locked) = await distributedLock.TryAcquireLockAsync(
                GetExportLockKey(entry.EntryId),
                _exportLockTimeout,
                _exportLockExpiration);

            if (!locked)
            {
                continue;
            }

            await using var acquiredLock = locker;

            var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(entry.ContentType);

            if (contentTypeDefinition == null)
            {
                await SaveEntryWithErrorAsync(session, clock, entry, localizer["The content definition was removed."]);
                continue;
            }

            entry.Status = ContentTransferEntryStatus.Processing;

            var progressPart = entry.As<ImportFileProcessStatsPart>();
            progressPart.Errors ??= [];

            try
            {
                var context = new ImportContentContext()
                {
                    ContentItem = await serviceProvider.GetRequiredService<IContentManager>().NewAsync(entry.ContentType),
                    ContentTypeDefinition = contentTypeDefinition,
                };

                var columns = await contentImportManager.GetColumnsAsync(context);
                var exportColumns = columns.Where(x => x.Type != ImportColumnType.ImportOnly).ToList();

                // Count total records for progress tracking.
                var filters = entry.As<ExportFilterPart>();
                var hasFilters = filters != null;

                var totalCountQuery = BuildExportQuery(
                    session, entry.ContentType, hasFilters,
                    filters?.LatestOnly ?? false,
                    filters?.AllVersions ?? false,
                    filters?.CreatedFrom, filters?.CreatedTo,
                    filters?.ModifiedFrom, filters?.ModifiedTo,
                    filters?.Owners);

                var totalCount = await totalCountQuery.CountAsync(cancellationToken);

                progressPart.TotalRecords = totalCount;

                // Write Excel file directly to a temp file using pagination (avoids memory accumulation).
                var fileName = entry.StoredFileName;
                var tempFilePath = Path.GetTempFileName();

                using var tempStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, FileOptions.DeleteOnClose);
                using (var spreadsheetDocument = SpreadsheetDocument.Create(tempStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = contentTypeDefinition.DisplayName?.Length > 31
                            ? contentTypeDefinition.DisplayName[..31]
                            : contentTypeDefinition.DisplayName,
                    };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // Write header row.
                    var headerRow = new Row() { RowIndex = 1 };
                    sheetData.Append(headerRow);

                    uint columnIndex = 1;
                    var columnNames = new List<string>();

                    foreach (var column in exportColumns)
                    {
                        var cell = new Cell()
                        {
                            CellReference = GetCellReference(columnIndex, 1),
                            DataType = CellValues.String,
                            CellValue = new CellValue(column.Name),
                        };
                        headerRow.Append(cell);
                        columnNames.Add(column.Name);
                        columnIndex++;
                    }

                    // Paginate content items and write each page directly.
                    uint rowIndex = 2;
                    var page = 0;
                    var contentManager = serviceProvider.GetRequiredService<IContentManager>();

                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        var pageQuery = BuildExportQuery(
                            session, entry.ContentType, hasFilters,
                            filters?.LatestOnly ?? false,
                            filters?.AllVersions ?? false,
                            filters?.CreatedFrom, filters?.CreatedTo,
                            filters?.ModifiedFrom, filters?.ModifiedTo,
                            filters?.Owners);

                        var contentItems = await pageQuery
                            .Skip(page * batchSize)
                            .Take(batchSize)
                            .ListAsync(cancellationToken);

                        var items = contentItems.ToList();

                        if (items.Count == 0)
                        {
                            break;
                        }

                        // Create a temporary DataTable for this batch only.
                        using var dataTable = new DataTable();

                        foreach (var colName in columnNames)
                        {
                            dataTable.Columns.Add(colName);
                        }

                        foreach (var contentItem in items)
                        {
                            var mapContext = new ContentExportContext()
                            {
                                ContentItem = contentItem,
                                ContentTypeDefinition = contentTypeDefinition,
                                Row = dataTable.NewRow(),
                            };

                            await contentImportManager.ExportAsync(mapContext);

                            // Write the row directly to the spreadsheet.
                            var row = new Row() { RowIndex = rowIndex };
                            sheetData.Append(row);

                            columnIndex = 1;

                            foreach (var colName in columnNames)
                            {
                                var cellValue = mapContext.Row[colName]?.ToString() ?? string.Empty;
                                var cell = new Cell()
                                {
                                    CellReference = GetCellReference(columnIndex, rowIndex),
                                    DataType = CellValues.String,
                                    CellValue = new CellValue(cellValue),
                                };
                                row.Append(cell);
                                columnIndex++;
                            }

                            rowIndex++;
                            progressPart.TotalProcessed++;
                        }

                        // Save progress after each page.
                        progressPart.CurrentRow = (int)rowIndex - 2;
                        entry.ProcessSaveUtc = clock.UtcNow;
                        entry.Put(progressPart);

                        session.Save(entry);
                        await session.SaveChangesAsync(cancellationToken);

                        page++;
                    }

                    workbookPart.Workbook.Save();
                }

                // Save the completed file to the file store.
                tempStream.Seek(0, SeekOrigin.Begin);
                await fileStore.CreateFileFromStreamAsync(fileName, tempStream, true);

                var nowUtc = clock.UtcNow;
                entry.ProcessSaveUtc = nowUtc;
                entry.CompletedUtc = nowUtc;
                entry.Status = ContentTransferEntryStatus.Completed;
                entry.Put(progressPart);

                session.Save(entry);
                await session.SaveChangesAsync(cancellationToken);

                // Send notification if the Notifications module is enabled.
                await TrySendExportNotificationAsync(serviceProvider, entry, contentTypeDefinition.DisplayName);
            }
            catch (Exception ex)
            {
                await SaveEntryWithErrorAsync(session, clock, entry, localizer["Error processing export: {0}", ex.Message]);
            }
        }
    }

    private static async Task TrySendExportNotificationAsync(
        IServiceProvider serviceProvider,
        ContentTransferEntry entry,
        string contentTypeName)
    {
        // Resolve IContentTransferNotificationHandler if available (registered when Notifications module is enabled).
        var notificationHandler = serviceProvider.GetService<IContentTransferNotificationHandler>();

        if (notificationHandler != null)
        {
            await notificationHandler.NotifyExportCompletedAsync(entry, contentTypeName);
        }
    }

    private static async Task SaveEntryWithErrorAsync(ISession session, IClock clock, ContentTransferEntry entry, string error)
    {
        entry.Status = ContentTransferEntryStatus.Failed;
        entry.Error = error;
        entry.CompletedUtc = clock.UtcNow;

        session.Save(entry);
        await session.SaveChangesAsync();
    }

    private static string GetCellReference(uint columnIndex, uint rowIndex)
    {
        var columnName = string.Empty;
        var dividend = columnIndex;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName + rowIndex;
    }

    private static string GetExportLockKey(string entryId)
        => $"ContentsTransfer_Export_{entryId}";

    private static IQuery<ContentItem> BuildExportQuery(
        ISession session,
        string contentTypeId,
        bool hasFilters,
        bool latestOnly,
        bool allVersions,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? modifiedFrom,
        DateTime? modifiedTo,
        string owners)
    {
        IQuery<ContentItem, ContentItemIndex> query;

        if (allVersions)
        {
            query = session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId);
        }
        else if (latestOnly)
        {
            query = session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId && x.Latest);
        }
        else
        {
            query = session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId && x.Published);
        }

        if (hasFilters)
        {
            if (createdFrom.HasValue)
            {
                query = query.Where(x => x.CreatedUtc >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(x => x.CreatedUtc <= createdTo.Value);
            }

            if (modifiedFrom.HasValue)
            {
                query = query.Where(x => x.ModifiedUtc >= modifiedFrom.Value);
            }

            if (modifiedTo.HasValue)
            {
                query = query.Where(x => x.ModifiedUtc <= modifiedTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(owners))
            {
                var ownerList = owners.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (ownerList.Length == 1)
                {
                    var owner = ownerList[0];
                    query = query.Where(x => x.Owner == owner);
                }
                else if (ownerList.Length > 1)
                {
                    var o0 = ownerList[0];
                    var o1 = ownerList.ElementAtOrDefault(1) ?? o0;
                    var o2 = ownerList.ElementAtOrDefault(2) ?? o0;
                    var o3 = ownerList.ElementAtOrDefault(3) ?? o0;
                    var o4 = ownerList.ElementAtOrDefault(4) ?? o0;

                    query = query.Where(x =>
                        x.Owner == o0 || x.Owner == o1 || x.Owner == o2 ||
                        x.Owner == o3 || x.Owner == o4);
                }
            }
        }

        return query.OrderBy(x => x.CreatedUtc);
    }
}
