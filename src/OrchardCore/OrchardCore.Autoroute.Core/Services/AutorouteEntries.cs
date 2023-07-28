using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Core.Model;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Autoroute.Core.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly IVolatileDocumentManager<AutorouteStateDocument> _autorouteStateManager;

        private ImmutableDictionary<string, AutorouteEntry> _paths = ImmutableDictionary<string, AutorouteEntry>.Empty;
        private ImmutableDictionary<string, AutorouteEntry> _contentItemIds = ImmutableDictionary<string, AutorouteEntry>.Empty;
        private readonly SemaphoreSlim _semaphore = new(1);

        private long _lastIndexId;
        private string _stateIdentifier;
        private bool _initialized;

        public AutorouteEntries(IVolatileDocumentManager<AutorouteStateDocument> autorouteStateManager)
        {
            _autorouteStateManager = autorouteStateManager;
            _contentItemIds = _contentItemIds.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByPathAsync(string path)
        {
            await EnsureInitializedAsync();

            if (_contentItemIds.TryGetValue(path.TrimEnd('/'), out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByContentItemIdAsync(string contentItemId)
        {
            await EnsureInitializedAsync();

            if (_paths.TryGetValue(contentItemId, out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task UpdateEntriesAsync()
        {
            await EnsureInitializedAsync();

            // Update the cache with a new state and then refresh entries as it would be done on a next request.
            await _autorouteStateManager.UpdateAsync(new AutorouteStateDocument(), afterUpdateAsync: RefreshEntriesAsync);
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeEntriesAsync();
            }
            else
            {
                var state = await _autorouteStateManager.GetOrCreateImmutableAsync();
                if (_stateIdentifier != state.Identifier)
                {
                    await RefreshEntriesAsync(state);
                }
            }
        }

        protected void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            // Evict all entries related to a container item from autoroute entries.
            // This is necessary to account for deletions, disabling of an item, or disabling routing of contained items.
            ILookup<string, AutorouteEntry> entriesByContainer = null;
            foreach (var entry in entries.Where(x => String.IsNullOrEmpty(x.ContainedContentItemId)))
            {
                entriesByContainer ??= _paths.Values
                    .Where(x => !String.IsNullOrEmpty(x.ContainedContentItemId))
                    .ToLookup(x => x.ContentItemId);

                if (!entriesByContainer.Contains(entry.ContentItemId))
                {
                    continue;
                }

                var entriesToRemove = entriesByContainer[entry.ContentItemId];

                _paths = _paths.RemoveRange(entriesToRemove.Select(x => x.ContainedContentItemId));
                _contentItemIds = _contentItemIds.RemoveRange(entriesToRemove.Select(x => x.Path));
            }

            foreach (var entry in entries)
            {
                if (_paths.TryGetValue(entry.ContentItemId, out var previousContainerEntry) &&
                    String.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    _contentItemIds = _contentItemIds.Remove(previousContainerEntry.Path);
                }

                if (!String.IsNullOrEmpty(entry.ContainedContentItemId) &&
                    _paths.TryGetValue(entry.ContainedContentItemId, out var previousContainedEntry))
                {
                    _contentItemIds = _contentItemIds.Remove(previousContainedEntry.Path);
                }

                _contentItemIds = _contentItemIds.SetItem(entry.Path, entry);

                if (!String.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    _paths = _paths.SetItem(entry.ContainedContentItemId, entry);
                }
                else
                {
                    _paths = _paths.SetItem(entry.ContentItemId, entry);
                }
            }
        }

        protected void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            foreach (var entry in entries)
            {
                // Evict all entries related to a container item from autoroute entries.
                var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId &&
                    !String.IsNullOrEmpty(x.ContainedContentItemId));

                _paths = _paths.RemoveRange(entriesToRemove.Select(x => x.ContainedContentItemId));
                _contentItemIds = _contentItemIds.RemoveRange(entriesToRemove.Select(x => x.Path));

                _paths = _paths.Remove(entry.ContentItemId);
                _contentItemIds = _contentItemIds.Remove(entry.Path);
            }
        }

        private async Task RefreshEntriesAsync(AutorouteStateDocument state)
        {
            if (_stateIdentifier == state.Identifier)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_stateIdentifier != state.Identifier)
                {
                    var indexes = await Session
                        .QueryIndex<AutoroutePartIndex>(i => i.Id > _lastIndexId)
                        .OrderBy(i => i.Id)
                        .ListAsync();

                    // A draft is indexed to check for conflicts, and to remove an entry, but only if an item is unpublished,
                    // so only if the entry 'DocumentId' matches, this because when a draft is saved more than once, the index
                    // is not updated for the published version that may be already scanned, so the entry may not be re-added.

                    var entriesToRemove = indexes
                        .Where(i => !i.Published || i.Path == null)
                        .SelectMany(i => _paths.Values.Where(e =>
                            // The item was removed.
                            ((!i.Published && !i.Latest) ||
                            // The part was disabled or removed.
                            (i.Path == null && i.Published) ||
                            // The item was unpublished.
                            (!i.Published && e.DocumentId == i.DocumentId)) &&
                            (e.ContentItemId == i.ContentItemId ||
                            e.ContainedContentItemId == i.ContentItemId)));

                    var entriesToAdd = indexes
                        .Where(i => i.Published && i.Path != null)
                        .Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath)
                        {
                            DocumentId = i.DocumentId
                        });

                    RemoveEntries(entriesToRemove);
                    AddEntries(entriesToAdd);

                    _lastIndexId = indexes.LastOrDefault()?.Id ?? 0;
                    _stateIdentifier = state.Identifier;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual async Task InitializeEntriesAsync()
        {
            if (_initialized)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    var state = await _autorouteStateManager.GetOrCreateImmutableAsync();

                    var indexes = await Session
                        .QueryIndex<AutoroutePartIndex>(i => i.Published && i.Path != null)
                        .OrderBy(i => i.Id)
                        .ListAsync();

                    var entries = indexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath)
                    {
                        DocumentId = i.DocumentId
                    });

                    AddEntries(entries);

                    _lastIndexId = indexes.LastOrDefault()?.Id ?? 0;
                    _stateIdentifier = state.Identifier;

                    _initialized = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();
    }
}
