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
        private const int MaxEventsCount = 100;

        private ImmutableDictionary<string, AutorouteEntry> _paths;
        public ImmutableDictionary<string, AutorouteEntry> _contentItemIds;

        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _initializeSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _eventsSemaphore = new SemaphoreSlim(1);

        private volatile string _lastEventId;
        private volatile bool _initialized;

        public AutorouteEntries()
        {
            _paths = ImmutableDictionary<string, AutorouteEntry>.Empty;
            _contentItemIds = ImmutableDictionary<string, AutorouteEntry>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByPathAsync(string path)
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }

            var events = await GetEventsAsync();

            if (_lastEventId != events.Identifier)
            {
                await HandleEventsAsync(events);
            }

            if (_contentItemIds.TryGetValue(path, out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task<(bool, AutorouteEntry)> TryGetEntryByContentItemIdAsync(string contentItemId)
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }

            var events = await GetEventsAsync();

            if (_lastEventId != events.Identifier)
            {
                await HandleEventsAsync(events);
            }

            if (_paths.TryGetValue(contentItemId, out var entry))
            {
                return (true, entry);
            }

            return (false, entry);
        }

        public async Task AddEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }

            await DocumentManager.UpdateAtomicAsync(async () =>
            {
                var events = await GetEventsAsync();

                events.Identifier = IdGenerator.GenerateId();
                events.List.Add(new AutorouteEvent()
                {
                    Name = "AddEntries",
                    Id = events.Identifier,
                    Entries = new List<AutorouteEntry>(entries)
                });

                var count = events.List.Count;
                if (count > MaxEventsCount)
                {
                    events.List = events.List.Skip(count - MaxEventsCount).ToList();
                }

                await HandleEventsAsync(events);
                return events;
            });
        }

        public async Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }

            await DocumentManager.UpdateAtomicAsync(async () =>
            {
                var events = await GetEventsAsync();

                events.Identifier = IdGenerator.GenerateId();
                events.List.Add(new AutorouteEvent()
                {
                    Name = "RemoveEntries",
                    Id = events.Identifier,
                    Entries = new List<AutorouteEntry>(entries)
                });

                var count = events.List.Count;
                if (count > MaxEventsCount)
                {
                    events.List = events.List.Skip(count - MaxEventsCount).ToList();
                }

                await HandleEventsAsync(events);
                return events;
            });
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
                if (_initialized)
                {
                    return;
                }

                var events = await GetEventsAsync();
                await InitializeEntriesAsync();
                _lastEventId = events.Identifier;
                _initialized = true;
            }
            finally
            {
                _initializeSemaphore.Release();
            }
        }

        private async Task HandleEventsAsync(AutorouteEventsDocument events)
        {
            if (_lastEventId == events.Identifier)
            {
                return;
            }

            await _eventsSemaphore.WaitAsync();
            try
            {
                if (_lastEventId == events.Identifier)
                {
                    return;
                }

                // If there is no event we may have handled and if the commands max count was reached.
                var lastEventIdExists = events.List.Any(x => x.Id == _lastEventId);
                if (!lastEventIdExists && events.List.Count == MaxEventsCount)
                {
                    // Re-init the local entries, as we don't know how many commands we may have lost.
                    _initialized = false;
                    await InitializeAsync();
                    return;
                }

                var lastEventIdFound = false;

                foreach (var @event in events.List)
                {
                    if (@event.Id == _lastEventId)
                    {
                        lastEventIdFound = true;
                        continue;
                    }

                    if (lastEventIdExists && !lastEventIdFound)
                    {
                        continue;
                    }

                    if (@event.Name == "AddEntries")
                    {
                        await AddEntriesInternalAsync(@event.Entries);
                        continue;
                    }

                    if (@event.Name == "RemoveEntries")
                    {
                        await RemoveEntriesInternalAsync(@event.Entries);
                    }
                }

                _lastEventId = events.Identifier;
            }
            finally
            {
                _eventsSemaphore.Release();
            }
        }

        private async Task AddEntriesInternalAsync(IEnumerable<AutorouteEntry> entries)
        {
            await _updateSemaphore.WaitAsync();
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
                _updateSemaphore.Release();
            }

        }

        private async Task RemoveEntriesInternalAsync(IEnumerable<AutorouteEntry> entries)
        {
            await _updateSemaphore.WaitAsync();
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
                _updateSemaphore.Release();
            }
        }

        /// <summary>
        /// Loads the autoroute events document for updating and that should not be cached.
        /// </summary>
        private Task<AutorouteEventsDocument> LoadEventsAsync() => DocumentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the autoroute events document for sharing and that should not be updated.
        /// </summary>
        private Task<AutorouteEventsDocument> GetEventsAsync() => DocumentManager.GetOrCreateImmutableAsync();

        protected virtual async Task InitializeEntriesAsync()
        {
            var indexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Published).ListAsync();
            var entries = indexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));
            await AddEntriesInternalAsync(entries);
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<AutorouteEventsDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteEventsDocument>>();
    }
}
