using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        public AutorouteEntries()
        {
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByPathAsync(string path)
        {
            var document = await GetDocumentAsync();

            if (document.ContentItemIds.TryGetValue(path, out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByContentItemIdAsync(string contentItemId)
        {
            var document = await GetDocumentAsync();

            if (document.Paths.TryGetValue(contentItemId, out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task AddEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            var document = await LoadDocumentAsync();
            AddEntries(document, entries);
            await DocumentManager.UpdateAsync(document);
        }

        public async Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            var document = await LoadDocumentAsync();
            RemoveEntries(document, entries);
            await DocumentManager.UpdateAsync(document);
        }

        private static void AddEntries(AutorouteDocument document, IEnumerable<AutorouteEntry> entries)
        {
            // Evict all entries related to a container item from autoroute entries.
            // This is necessary to account for deletions, disabling of an item, or disabling routing of contained items.
            foreach (var entry in entries.Where(x => String.IsNullOrEmpty(x.ContainedContentItemId)))
            {
                var entriesToRemove = document.Paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                foreach (var entryToRemove in entriesToRemove)
                {
                    document.Paths.Remove(entryToRemove.ContainedContentItemId);
                    document.ContentItemIds.Remove(entryToRemove.Path);
                }
            }

            foreach (var entry in entries)
            {
                if (document.Paths.TryGetValue(entry.ContentItemId, out var previousContainerEntry) && String.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    document.ContentItemIds.Remove(previousContainerEntry.Path);
                }

                if (!String.IsNullOrEmpty(entry.ContainedContentItemId) && document.Paths.TryGetValue(entry.ContainedContentItemId, out var previousContainedEntry))
                {
                    document.ContentItemIds.Remove(previousContainedEntry.Path);
                }

                document.ContentItemIds[entry.Path] = entry;

                if (!String.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    document.Paths[entry.ContainedContentItemId] = entry;
                }
                else
                {
                    document.Paths[entry.ContentItemId] = entry;
                }
            }
        }

        private static void RemoveEntries(AutorouteDocument document, IEnumerable<AutorouteEntry> entries)
        {
            foreach (var entry in entries)
            {
                // Evict all entries related to a container item from autoroute entries.
                var entriesToRemove = document.Paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                foreach (var entryToRemove in entriesToRemove)
                {
                    document.Paths.Remove(entryToRemove.ContainedContentItemId);
                    document.ContentItemIds.Remove(entryToRemove.Path);
                }

                document.Paths.Remove(entry.ContentItemId);
                document.ContentItemIds.Remove(entry.Path);
            }
        }

        /// <summary>
        /// Loads the autoroute document for updating and that should not be cached.
        /// </summary>
        private Task<AutorouteDocument> LoadDocumentAsync() => DocumentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the autoroute document for sharing and that should not be updated.
        /// </summary>
        private Task<AutorouteDocument> GetDocumentAsync() => DocumentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        protected virtual async Task<AutorouteDocument> CreateDocumentAsync()
        {
            var indexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Published).ListAsync();
            var entries = indexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));

            var document = new AutorouteDocument();
            AddEntries(document, entries);

            return document;
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<AutorouteDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteDocument>>();
    }
}
