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

        private readonly SemaphoreSlim _entriesSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _initializeSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private AutorouteStateDocument _state = new AutorouteStateDocument();

        private int _autoroutePartIndexLastId;
        private int _contentItemIndexLastId;
        private bool _initialized;

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

        public async Task AddEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            await EnsureInitializedAsync();

            await AutorouteStateManager.UpdateAtomicAsync(async () =>
            {
                await RefreshLocalEntriesAsync();
                await AddLocalEntriesAsync(entries);
                return _state = new AutorouteStateDocument();
            });
        }

        public async Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            await EnsureInitializedAsync();

            await AutorouteStateManager.UpdateAtomicAsync(async () =>
            {
                await RefreshLocalEntriesAsync();
                await RemoveLocalEntriesAsync(entries);
                return _state = new AutorouteStateDocument();
            });
        }

        private async Task EnsureInitializedAsync()
        {
            var document = await GetStateDocumentAsync();

            if (!_initialized)
            {
                await InitializeAsync();
                return;
            }

            if (_state.Identifier != document.Identifier)
            {
                await RefreshLocalEntriesAsync();
            }
        }

        private async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            await _initializeSemaphore.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    await InitializeLocalEntriesAsync();
                    _initialized = true;
                }

            }
            finally
            {
                _initializeSemaphore.Release();
            }
        }

        private async Task AddLocalEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            await _entriesSemaphore.WaitAsync();
            try
            {
                // Evict all entries related to a container item from autoroute entries.
                // This is necessary to account for deletions, disabling of an item, or disabling routing of contained items.
                foreach (var entry in entries.Where(x => String.IsNullOrEmpty(x.ContainedContentItemId)))
                {
                    var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));

                    _paths = _paths.RemoveRange(entriesToRemove.Select(x => x.ContainedContentItemId));
                    _contentItemIds = _contentItemIds.RemoveRange(entriesToRemove.Select(x => x.Path));

                    foreach (var entryToRemove in entriesToRemove)
                    {
                        _paths = _paths.Remove(entryToRemove.ContainedContentItemId);
                        _contentItemIds = _contentItemIds.Remove(entryToRemove.Path);
                    }
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
            finally
            {
                _entriesSemaphore.Release();
            }

        }

        private async Task RemoveLocalEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            await _entriesSemaphore.WaitAsync();
            try
            {
                foreach (var entry in entries)
                {
                    // Evict all entries related to a container item from autoroute entries.
                    var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                    foreach (var entryToRemove in entriesToRemove)
                    {
                        _paths = _paths.Remove(entryToRemove.ContainedContentItemId);
                        _contentItemIds = _contentItemIds.Remove(entryToRemove.Path);
                    }

                    _paths = _paths.Remove(entry.ContentItemId);
                    _contentItemIds = _contentItemIds.Remove(entry.Path);
                }
            }
            finally
            {
                _entriesSemaphore.Release();
            }
        }

        protected virtual async Task RefreshLocalEntriesAsync()
        {
            var document = await LoadStateDocumentAsync();

            if (_state.Identifier == document.Identifier)
            {
                return;
            }

            await _updateSemaphore.WaitAsync();
            try
            {
                if (_state.Identifier != document.Identifier)
                {
                    _state = document;

                    var autorouteIndexes = await Session.QueryIndex<AutoroutePartIndex>(
                        i => i.Id > _autoroutePartIndexLastId && i.Published)
                        .ListAsync();

                    var itemIndexes = await Session.QueryIndex<ContentItemIndex>(
                        i => i.Id > _contentItemIndexLastId)
                        .ListAsync();

                    _autoroutePartIndexLastId = autorouteIndexes.LastOrDefault()?.Id ?? 0;
                    _contentItemIndexLastId = itemIndexes.LastOrDefault()?.Id ?? 0;

                    var valideIds = itemIndexes.Where(i => i.Published || i.Latest).Select(i => i.ContentItemId);
                    var removedIds = itemIndexes.Where(i => !valideIds.Contains(i.ContentItemId)).Select(i => i.ContentItemId).Distinct();

                    var entriesToRemove = new List<AutorouteEntry>();
                    foreach (var contentItemId in removedIds)
                    {
                        entriesToRemove.AddRange(
                            _paths.Values.Where(entry => entry.ContentItemId == contentItemId ||
                                entry.ContainedContentItemId == contentItemId));
                    }

                    var entriesToAdd = autorouteIndexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));

                    await RemoveLocalEntriesAsync(entriesToRemove);
                    await AddLocalEntriesAsync(entriesToAdd);
                }
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        protected virtual async Task InitializeLocalEntriesAsync()
        {
            _state = await LoadStateDocumentAsync();

            var autorouteIndexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Published).ListAsync();
            var itemLastIndex = await Session.QueryIndex<ContentItemIndex>().OrderByDescending(i => i.Id).FirstOrDefaultAsync();

            _autoroutePartIndexLastId = autorouteIndexes.LastOrDefault()?.Id ?? 0;
            _contentItemIndexLastId = itemLastIndex?.Id ?? 0;

            var entries = autorouteIndexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));
            await AddLocalEntriesAsync(entries);
        }

        private static Task<AutorouteStateDocument> LoadStateDocumentAsync() => AutorouteStateManager.GetOrCreateMutableAsync();

        private static Task<AutorouteStateDocument> GetStateDocumentAsync() => AutorouteStateManager.GetOrCreateImmutableAsync();

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<AutorouteStateDocument> AutorouteStateManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteStateDocument>>();
    }
}
