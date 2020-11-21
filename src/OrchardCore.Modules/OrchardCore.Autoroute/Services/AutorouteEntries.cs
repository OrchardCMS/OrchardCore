using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
        private ImmutableDictionary<string, AutorouteEntry> _paths = ImmutableDictionary<string, AutorouteEntry>.Empty;
        private ImmutableDictionary<string, AutorouteEntry> _contentItemIds = ImmutableDictionary<string, AutorouteEntry>.Empty;

        private AutorouteStateDocument _state = new AutorouteStateDocument();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private bool _initialized;
        private int _lastIndexId;

        public AutorouteEntries()
        {
            _contentItemIds = _contentItemIds.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByPathAsync(string path)
        {
            await EnsureInitializedAsync();

            if (_contentItemIds.TryGetValue(path, out var entry))
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

            await AutorouteStateManager.UpdateAtomicAsync(async () =>
            {
                var document = await LoadStateDocumentAsync();
                await RefreshAsync(document);
                return document;
            });
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            // Evict all entries related to a container item from autoroute entries.
            // This is necessary to account for deletions, disabling of an item, or disabling routing of contained items.
            foreach (var entry in entries.Where(x => String.IsNullOrEmpty(x.ContainedContentItemId)))
            {
                var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                _paths = _paths.RemoveRange(entriesToRemove.Select(x => x.ContainedContentItemId));
                _contentItemIds = _contentItemIds.RemoveRange(entriesToRemove.Select(x => x.Path));
            }

            foreach (var entry in entries)
            {
                if (_paths.TryGetValue(entry.ContentItemId, out var previousContainerEntry) && String.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    _contentItemIds = _contentItemIds.Remove(previousContainerEntry.Path);
                }

                if (!String.IsNullOrEmpty(entry.ContainedContentItemId) && _paths.TryGetValue(entry.ContainedContentItemId, out var previousContainedEntry))
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

        public void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            foreach (var entry in entries)
            {
                // Evict all entries related to a container item from autoroute entries.
                var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                _paths = _paths.RemoveRange(entriesToRemove.Select(x => x.ContainedContentItemId));
                _contentItemIds = _contentItemIds.RemoveRange(entriesToRemove.Select(x => x.Path));

                _paths = _paths.Remove(entry.ContentItemId);
                _contentItemIds = _contentItemIds.Remove(entry.Path);
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
                return;
            }

            var document = await GetStateDocumentAsync();
            if (_state.Identifier != document.Identifier)
            {
                await RefreshAsync(document);
            }
        }

        private async Task RefreshAsync(AutorouteStateDocument document)
        {
            if (_state.Identifier == document.Identifier)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_state.Identifier != document.Identifier)
                {
                    var indexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Id > _lastIndexId).ListAsync();
                    _lastIndexId = indexes.LastOrDefault()?.Id ?? 0;

                    var entriesToRemove = new List<AutorouteEntry>();
                    var removedIds = indexes.Where(i => !i.Published || i.Path == null).Select(i => i.ContentItemId).Distinct();
                    foreach (var id in removedIds)
                    {
                        entriesToRemove.AddRange(_paths.Values.Where(e => e.ContentItemId == id || e.ContainedContentItemId == id));
                    }

                    var entriesToAdd = indexes.Where(i => i.Published && i.Path != null).Select(i => new AutorouteEntry
                    (
                        i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath
                    ));

                    RemoveEntries(entriesToRemove);
                    AddEntries(entriesToAdd);

                    _state = document;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual async Task InitializeAsync()
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
                    _state = await GetStateDocumentAsync();

                    var indexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Published && i.Path != null).ListAsync();
                    _lastIndexId = indexes.LastOrDefault()?.Id ?? 0;

                    var entries = indexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));
                    AddEntries(entries);

                    _initialized = true;
                }

            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static Task<AutorouteStateDocument> LoadStateDocumentAsync() => AutorouteStateManager.GetOrCreateMutableAsync();
        private static Task<AutorouteStateDocument> GetStateDocumentAsync() => AutorouteStateManager.GetOrCreateImmutableAsync();

        private static IVolatileDocumentManager<AutorouteStateDocument> AutorouteStateManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteStateDocument>>();
    }
}
