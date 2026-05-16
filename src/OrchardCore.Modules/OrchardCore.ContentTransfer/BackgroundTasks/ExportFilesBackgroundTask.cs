using System.Data;
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

            var progressPart = entry.GetOrCreate<ImportFileProcessStatsPart>();
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
                var hasFilters = entry.TryGet<ExportFilterPart>(out var filters);

                var totalCountQuery = BuildExportQuery(
                    session, entry.ContentType, hasFilters,
                    filters?.LatestOnly ?? false,
                    filters?.AllVersions ?? false,
                    filters?.CreatedFrom, filters?.CreatedTo,
                    filters?.ModifiedFrom, filters?.ModifiedTo,
                    filters?.Owners);

                var totalCount = await totalCountQuery.CountAsync(cancellationToken);

                progressPart.TotalRecords = totalCount;

                // Resolve file format provider from the stored file name extension.
                var formatProviders = serviceProvider.GetServices<IContentTransferFileFormatProvider>();
                var formatProvider = formatProviders.FirstOrDefault(p => p.CanHandle(entry.StoredFileName))
                    ?? formatProviders.First(p => p.FileExtension == ".xlsx");

                var fileName = entry.StoredFileName;
                var tempFilePath = Path.GetTempFileName();

                using var tempStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, FileOptions.DeleteOnClose);
                var columnNames = exportColumns.Select(c => c.Name).ToList();

                using (var writer = formatProvider.CreateWriter(tempStream, contentTypeDefinition.DisplayName))
                {
                    writer.WriteHeader(columnNames);

                    // Paginate content items and write each page directly.
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

                            var rowValues = new List<string>(columnNames.Count);
                            foreach (var colName in columnNames)
                            {
                                rowValues.Add(mapContext.Row[colName]?.ToString() ?? string.Empty);
                            }

                            writer.WriteRow(rowValues);
                            progressPart.TotalProcessed++;
                        }

                        // Save progress after each page.
                        progressPart.CurrentRow = progressPart.TotalProcessed;
                        entry.ProcessSaveUtc = clock.UtcNow;
                        entry.Put(progressPart);

                        session.Save(entry);
                        await session.SaveChangesAsync(cancellationToken);

                        page++;
                    }

                    writer.Flush();
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
