using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.ContentTransfer.Models;
using OrchardCore.Entities;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ContentTransfer.BackgroundTasks;

[BackgroundTask(
    Title = "Imported Files Processor",
    Schedule = "*/10 * * * *",
    Description = "Regularly check for imported files and process them.",
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class ImportFilesBackgroundTask : IBackgroundTask
{
    private static readonly TimeSpan _importLockTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _importLockExpiration = TimeSpan.FromMinutes(30);

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => ProcessEntriesAsync(serviceProvider, cancellationToken);

    internal static async Task ProcessEntriesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken, string entryId = null)
    {
        var session = serviceProvider.GetRequiredService<ISession>();
        var distributedLock = serviceProvider.GetRequiredService<IDistributedLock>();

        var entries = await session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
                (x.Status == ContentTransferEntryStatus.New || x.Status == ContentTransferEntryStatus.Processing)
                && x.Direction == ContentTransferDirection.Import
                && (entryId == null || x.EntryId == entryId))
            .OrderBy(x => x.CreatedUtc)
            .ListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            (var locker, var locked) = await distributedLock.TryAcquireLockAsync(
                GetImportLockKey(entry.EntryId),
                _importLockTimeout,
                _importLockExpiration);

            if (!locked)
            {
                continue;
            }

            await using var acquiredLock = locker;
            await using var scope = serviceProvider.CreateAsyncScope();
            await ProcessEntryAsync(scope.ServiceProvider, entry.EntryId, cancellationToken);
        }
    }

    private static async Task ProcessEntryAsync(IServiceProvider serviceProvider, string entryId, CancellationToken cancellationToken)
    {
        var session = serviceProvider.GetRequiredService<ISession>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();
        var contentImportOptions = serviceProvider.GetRequiredService<IOptions<ContentImportOptions>>().Value;
        var localizer = serviceProvider.GetRequiredService<IStringLocalizer<ImportFilesBackgroundTask>>();
        var fileStore = serviceProvider.GetRequiredService<IContentTransferFileStore>();
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();
        var contentImportManager = serviceProvider.GetRequiredService<IContentImportManager>();
        var entry = await session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.EntryId == entryId).FirstOrDefaultAsync(cancellationToken);

        if (entry == null || entry.Status == ContentTransferEntryStatus.Canceled || entry.Status == ContentTransferEntryStatus.CanceledWithImportedRecords)
        {
            return;
        }

        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(entry.ContentType);

        if (contentTypeDefinition == null)
        {
            await SaveEntryWithErrorAsync(session, clock, entry, localizer["The content definition was removed."], cancellationToken);
            return;
        }

        var fileInfo = await fileStore.GetFileInfoAsync(entry.StoredFileName);

        if (fileInfo == null || fileInfo.Length == 0)
        {
            await SaveEntryWithErrorAsync(session, clock, entry, localizer["The import file no longer exists."], cancellationToken);
            return;
        }

        var batchSize = contentImportOptions.ImportBatchSize < 1
            ? ContentImportOptions.DefaultImportBatchSize
            : contentImportOptions.ImportBatchSize;

        entry.Status = ContentTransferEntryStatus.Processing;
        entry.Error = null;
        entry.CompletedUtc = null;

        var progressPart = entry.GetOrCreate<ImportFileProcessStatsPart>();
        progressPart.Errors ??= [];
        progressPart.ErrorMessages ??= [];
        entry.Put(progressPart);

        session.Save(entry);
        await session.SaveChangesAsync(cancellationToken);

        await using var fileStream = await fileStore.GetFileStreamAsync(fileInfo);

        try
        {
            var formatProviders = serviceProvider.GetServices<IContentTransferFileFormatProvider>();
            var formatProvider = formatProviders.FirstOrDefault(p => p.CanHandle(entry.StoredFileName))
                ?? throw new InvalidOperationException(localizer["Unsupported file format: {0}", Path.GetExtension(entry.StoredFileName)]);

            await ProcessFileInBatchesAsync(
                serviceProvider,
                fileStream,
                formatProvider,
                entry,
                progressPart,
                contentTypeDefinition,
                contentManager,
                contentImportManager,
                session,
                clock,
                batchSize,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await SaveEntryWithErrorAsync(session, clock, entry, localizer["Error processing file: {0}", ex.Message], cancellationToken);
            return;
        }

        if (await IsImportCanceledAsync(serviceProvider, entry.EntryId, cancellationToken))
        {
            entry.Status = GetCanceledStatus(progressPart);
            entry.ProcessSaveUtc = clock.UtcNow;
            entry.CompletedUtc = clock.UtcNow;
            entry.Put(progressPart);
            session.Save(entry);
            await session.SaveChangesAsync(cancellationToken);
            return;
        }

        var nowUtc = clock.UtcNow;
        entry.ProcessSaveUtc = nowUtc;
        entry.CompletedUtc = nowUtc;
        entry.Status = progressPart.Errors.Count > 0
            ? ContentTransferEntryStatus.CompletedWithErrors
            : ContentTransferEntryStatus.Completed;
        entry.Put(progressPart);

        session.Save(entry);
        await session.SaveChangesAsync(cancellationToken);
    }

    private static async Task ProcessFileInBatchesAsync(
        IServiceProvider serviceProvider,
        Stream stream,
        IContentTransferFileFormatProvider formatProvider,
        ContentTransferEntry entry,
        ImportFileProcessStatsPart progressPart,
        ContentTypeDefinition contentTypeDefinition,
        IContentManager contentManager,
        IContentImportManager contentImportManager,
        ISession session,
        IClock clock,
        int batchSize,
        CancellationToken cancellationToken)
    {
        using var reader = formatProvider.CreateReader(stream);

        var columnNames = reader.GetColumnNames();
        var dataTable = new DataTable();

        foreach (var columnName in columnNames)
        {
            var name = columnName;
            var occurrences = dataTable.Columns.Cast<DataColumn>().Count(c => c.ColumnName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (occurrences > 0)
            {
                name += " " + (occurrences + 1);
            }

            dataTable.Columns.Add(new DataColumn(name));
        }

        progressPart.TotalRecords = reader.GetRowCount();

        var indexOfKeyColumn = dataTable.Columns.IndexOf(nameof(ContentItem.ContentItemId));
        var indexOfVersionKeyColumn = dataTable.Columns.IndexOf(nameof(ContentItem.ContentItemVersionId));
        var newRecords = new Dictionary<int, DataRow>();
        var existingVersionRows = new Dictionary<string, KeyValuePair<int, DataRow>>(StringComparer.OrdinalIgnoreCase);
        var existingRows = new Dictionary<string, KeyValuePair<int, DataRow>>(StringComparer.OrdinalIgnoreCase);

        var rowIndex = 1;

        foreach (var rowValues in reader.ReadRows())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (await IsImportCanceledAsync(serviceProvider, entry.EntryId, cancellationToken))
            {
                break;
            }

            if (rowIndex <= progressPart.CurrentRow)
            {
                rowIndex++;
                continue;
            }

            progressPart.TotalProcessed++;
            progressPart.CurrentRow = rowIndex;

            var dataRow = dataTable.NewRow();
            var isEmpty = true;

            for (var colIndex = 0; colIndex < rowValues.Length && colIndex < dataTable.Columns.Count; colIndex++)
            {
                var value = rowValues[colIndex];
                dataRow[colIndex] = value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    isEmpty = false;
                }
            }

            if (!isEmpty)
            {
                if (contentTypeDefinition.IsVersionable() && indexOfVersionKeyColumn > -1)
                {
                    var contentItemVersionId = dataRow[indexOfVersionKeyColumn]?.ToString()?.Trim();

                    if (!string.IsNullOrEmpty(contentItemVersionId))
                    {
                        existingVersionRows[contentItemVersionId] = new KeyValuePair<int, DataRow>(rowIndex, dataRow);
                    }
                    else if (indexOfKeyColumn > -1)
                    {
                        var contentItemId = dataRow[indexOfKeyColumn]?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(contentItemId))
                        {
                            existingRows[contentItemId] = new KeyValuePair<int, DataRow>(rowIndex, dataRow);
                        }
                        else
                        {
                            newRecords[rowIndex] = dataRow;
                        }
                    }
                    else
                    {
                        newRecords[rowIndex] = dataRow;
                    }
                }
                else if (indexOfKeyColumn > -1)
                {
                    var contentItemId = dataRow[indexOfKeyColumn]?.ToString()?.Trim();

                    if (!string.IsNullOrEmpty(contentItemId))
                    {
                        existingRows[contentItemId] = new KeyValuePair<int, DataRow>(rowIndex, dataRow);
                    }
                    else
                    {
                        newRecords[rowIndex] = dataRow;
                    }
                }
                else
                {
                    newRecords[rowIndex] = dataRow;
                }
            }

            if (newRecords.Count + existingRows.Count + existingVersionRows.Count >= batchSize)
            {
                await ProcessBatchAsync(
                    serviceProvider,
                    entry,
                    dataTable,
                    newRecords,
                    existingVersionRows,
                    existingRows,
                    contentTypeDefinition,
                    contentManager,
                    contentImportManager,
                    progressPart,
                    session,
                    clock,
                    cancellationToken);

                newRecords.Clear();
                existingVersionRows.Clear();
                existingRows.Clear();
                dataTable.Rows.Clear();
            }

            rowIndex++;
        }

        if (newRecords.Count + existingRows.Count + existingVersionRows.Count > 0)
        {
            await ProcessBatchAsync(
                serviceProvider,
                entry,
                dataTable,
                newRecords,
                existingVersionRows,
                existingRows,
                contentTypeDefinition,
                contentManager,
                contentImportManager,
                progressPart,
                session,
                clock,
                cancellationToken);
        }

        dataTable.Dispose();
    }

    private static async Task ProcessBatchAsync(
        IServiceProvider serviceProvider,
        ContentTransferEntry entry,
        DataTable dataTable,
        Dictionary<int, DataRow> newRecords,
        Dictionary<string, KeyValuePair<int, DataRow>> existingVersionRows,
        Dictionary<string, KeyValuePair<int, DataRow>> existingRows,
        ContentManagement.Metadata.Models.ContentTypeDefinition contentTypeDefinition,
        IContentManager contentManager,
        IContentImportManager contentImportManager,
        ImportFileProcessStatsPart progressPart,
        ISession session,
        IClock clock,
        CancellationToken cancellationToken)
    {
        if (existingVersionRows.Count > 0)
        {
            foreach (var existingVersionRow in existingVersionRows)
            {
                if (await IsImportCanceledAsync(serviceProvider, entry.EntryId, cancellationToken))
                {
                    return;
                }

                var contentItem = await contentManager.GetVersionAsync(existingVersionRow.Key);
                var isNew = contentItem == null;

                if (isNew)
                {
                    contentItem = await contentManager.NewAsync(entry.ContentType);
                }

                await ProcessRowAsync(
                    entry,
                    contentItem,
                    existingVersionRow.Value.Key,
                    existingVersionRow.Value.Value,
                    dataTable.Columns,
                    contentTypeDefinition,
                    contentManager,
                    contentImportManager,
                    progressPart,
                    isNew);
            }
        }

        if (existingRows.Count > 0)
        {
            var existingContentItems = (await contentManager.GetAsync(existingRows.Keys, VersionOptions.DraftRequired))
                .ToDictionary(x => x.ContentItemId);

            foreach (var existingRow in existingRows)
            {
                if (await IsImportCanceledAsync(serviceProvider, entry.EntryId, cancellationToken))
                {
                    return;
                }

                var isNew = false;

                if (!existingContentItems.TryGetValue(existingRow.Key, out var contentItem))
                {
                    contentItem = await contentManager.NewAsync(entry.ContentType);
                    isNew = true;
                }

                await ProcessRowAsync(
                    entry,
                    contentItem,
                    existingRow.Value.Key,
                    existingRow.Value.Value,
                    dataTable.Columns,
                    contentTypeDefinition,
                    contentManager,
                    contentImportManager,
                    progressPart,
                    isNew);
            }
        }

        foreach (var record in newRecords)
        {
            if (await IsImportCanceledAsync(serviceProvider, entry.EntryId, cancellationToken))
            {
                return;
            }

            await ProcessRowAsync(
                entry,
                await contentManager.NewAsync(entry.ContentType),
                record.Key,
                record.Value,
                dataTable.Columns,
                contentTypeDefinition,
                contentManager,
                contentImportManager,
                progressPart,
                true);
        }

        entry.ProcessSaveUtc = clock.UtcNow;
        entry.Put(progressPart);

        session.Save(entry);
        await session.SaveChangesAsync(cancellationToken);
    }

    private static async Task ProcessRowAsync(
        ContentTransferEntry entry,
        ContentItem contentItem,
        int rowIndex,
        DataRow row,
        DataColumnCollection columns,
        ContentManagement.Metadata.Models.ContentTypeDefinition contentTypeDefinition,
        IContentManager contentManager,
        IContentImportManager contentImportManager,
        ImportFileProcessStatsPart progressPart,
        bool isNew)
    {
        try
        {
            var mapContext = new ContentImportContext()
            {
                ContentItem = contentItem,
                ContentTypeDefinition = contentTypeDefinition,
                Columns = columns,
                Row = row,
            };

            await contentImportManager.ImportAsync(mapContext);

            var validationResult = await contentManager.ValidateAsync(mapContext.ContentItem);

            if (!validationResult.Succeeded)
            {
                AddRowError(progressPart, rowIndex, FormatValidationErrors(validationResult));
                return;
            }

            mapContext.ContentItem.Owner = entry.Owner;
            mapContext.ContentItem.Author = entry.Author;

            if (isNew)
            {
                await contentManager.CreateAsync(mapContext.ContentItem, VersionOptions.DraftRequired);
            }
            else
            {
                await contentManager.UpdateAsync(mapContext.ContentItem);
            }

            await contentManager.PublishAsync(mapContext.ContentItem);
            progressPart.ImportedCount++;

            progressPart.Errors.Remove(rowIndex);
            progressPart.ErrorMessages.Remove(rowIndex);
        }
        catch (Exception ex)
        {
            AddRowError(progressPart, rowIndex, ex.Message);
        }
    }

    private static void AddRowError(ImportFileProcessStatsPart progressPart, int rowIndex, string errorMessage)
    {
        progressPart.Errors ??= [];
        progressPart.ErrorMessages ??= [];

        progressPart.Errors.Add(rowIndex);
        progressPart.ErrorMessages[rowIndex] = string.IsNullOrWhiteSpace(errorMessage)
            ? "The row failed to import."
            : errorMessage;
    }

    private static string FormatValidationErrors(ContentValidateResult validationResult)
    {
        var messages = validationResult.Errors
            .Select(error =>
            {
                if (error.MemberNames != null && error.MemberNames.Any())
                {
                    return $"{string.Join(", ", error.MemberNames)}: {error.ErrorMessage}";
                }

                return error.ErrorMessage;
            })
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return messages.Length > 0
            ? string.Join("; ", messages)
            : "The row failed validation.";
    }

    private static async Task SaveEntryWithErrorAsync(ISession session, IClock clock, ContentTransferEntry entry, string error, CancellationToken cancellationToken)
    {
        entry.Status = ContentTransferEntryStatus.Failed;
        entry.Error = error;
        entry.CompletedUtc = clock.UtcNow;

        session.Save(entry);
        await session.SaveChangesAsync(cancellationToken);
    }

    private static string GetImportLockKey(string entryId)
        => $"ContentsTransfer_Import_{entryId}";

    private static async Task<bool> IsImportCanceledAsync(IServiceProvider serviceProvider, string entryId, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<ISession>();
        var currentEntry = await session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.EntryId == entryId).FirstOrDefaultAsync(cancellationToken);

        return currentEntry?.Status == ContentTransferEntryStatus.Canceled
            || currentEntry?.Status == ContentTransferEntryStatus.CanceledWithImportedRecords;
    }

    private static ContentTransferEntryStatus GetCanceledStatus(ImportFileProcessStatsPart progressPart)
        => (progressPart?.ImportedCount ?? 0) > 0
            ? ContentTransferEntryStatus.CanceledWithImportedRecords
            : ContentTransferEntryStatus.Canceled;
}
