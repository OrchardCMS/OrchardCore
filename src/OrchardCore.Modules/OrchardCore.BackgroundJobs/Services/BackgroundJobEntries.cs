using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobEntries : IBackgroundJobEntries
    {
        public async ValueTask<IEnumerable<BackgroundJobEntry>> GetEntriesAsync()
        {
            var document = await GetDocumentAsync();

            return document.Entries.Values;
        }

        public async ValueTask RemoveEntryAsync(string backgroundJobId)
        {
            var document = await LoadDocumentAsync();
            if (document.Entries.Remove(backgroundJobId))
            {
                await DocumentManager.UpdateAsync(document);
            }
        }

        public async ValueTask AddOrUpdateEntryAsync(BackgroundJobEntry entry)
        {
            if (entry.Status >= BackgroundJobStatus.Executed)
            {
                throw new InvalidOperationException($"Invalid status {entry.Status} for background job entry");
            }

            var document = await LoadDocumentAsync();
            
            document.Entries[entry.BackgroundJobId] = entry;
            await DocumentManager.UpdateAsync(document);
        }

        /// <summary>
        /// Loads the background job document for updating and that should not be cached.
        /// </summary>
        private Task<BackgroundJobDocument> LoadDocumentAsync() => DocumentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the background job document for sharing and that should not be updated.
        /// </summary>
        private Task<BackgroundJobDocument> GetDocumentAsync() => DocumentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        private async Task<BackgroundJobDocument> CreateDocumentAsync()
        {
            var indexes = await Session.QueryIndex<BackgroundJobIndex>(i => i.Status < BackgroundJobStatus.Executed).ListAsync();
            var entries = indexes.Select(i => new BackgroundJobEntry(i.BackgroundJobId, i.Name, i.Status, i.ExecutionUtc.GetValueOrDefault()));

            var document = new BackgroundJobDocument();
            foreach (var entry in entries)
            {
                document.Entries[entry.BackgroundJobId] = entry;
            }

            return document;
        }


        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<BackgroundJobDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<BackgroundJobDocument>>();
    }
}
