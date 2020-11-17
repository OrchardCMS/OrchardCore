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
        public ImmutableDictionary<string, AutorouteEntry> _contentItemIds = ImmutableDictionary<string, AutorouteEntry>.Empty;

        private readonly SemaphoreSlim _entriesSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _initializeSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _commandsSemaphore = new SemaphoreSlim(1);

        private string _lastCommandId;
        private bool _initialized;

        public AutorouteEntries()
        {
            _contentItemIds = _contentItemIds.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads the autoroute commands document for updating and that should not be cached.
        /// </summary>
        private Task<AutorouteCommandsDocument> LoadCommandsDocumentAsync() => DocumentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the autoroute commands document for sharing and that should not be updated.
        /// </summary>
        private Task<AutorouteCommandsDocument> GetCommandsDocumentAsync() => DocumentManager.GetOrCreateImmutableAsync();

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

            await DocumentManager.UpdateAtomicAsync(async () =>
            {
                var document = await LoadCommandsDocumentAsync();
                document.AddCommand(AutorouteCommand.AddEntries, entries);
                await UpdateLocalEntriesAsync(document);
                return document;
            });
        }

        public async Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            await EnsureInitializedAsync();

            await DocumentManager.UpdateAtomicAsync(async () =>
            {
                var document = await LoadCommandsDocumentAsync();
                document.AddCommand(AutorouteCommand.RemoveEntries, entries);
                await UpdateLocalEntriesAsync(document);
                return document;
            });
        }

        private async Task EnsureInitializedAsync()
        {
            var document = await GetCommandsDocumentAsync();

            if (!_initialized)
            {
                await InitializeAsync();
            }

            if (_lastCommandId != document.Identifier)
            {
                await UpdateLocalEntriesAsync(document);
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
                    var document = await GetCommandsDocumentAsync();
                    await InitializeLocalEntriesAsync();
                    _lastCommandId = document.Identifier;
                    _initialized = true;
                }

            }
            finally
            {
                _initializeSemaphore.Release();
            }
        }

        private async Task UpdateLocalEntriesAsync(AutorouteCommandsDocument document)
        {
            if (_lastCommandId == document.Identifier)
            {
                return;
            }

            await _commandsSemaphore.WaitAsync();
            try
            {
                if (_lastCommandId != document.Identifier)
                {
                    if (!document.TryGetLastCommands(_lastCommandId, out var lastCommands))
                    {
                        await InitializeLocalEntriesAsync();
                        _lastCommandId = document.Identifier;
                        return;
                    }

                    foreach (var command in lastCommands)
                    {
                        if (command.Name == AutorouteCommand.Obsolete)
                        {
                            continue;
                        }

                        if (command.Name == AutorouteCommand.AddEntries)
                        {
                            await AddLocalEntriesAsync(command.Entries);
                        }

                        if (command.Name == AutorouteCommand.RemoveEntries)
                        {
                            await RemoveLocalEntriesAsync(command.Entries);
                        }
                    }

                    _lastCommandId = document.Identifier;
                }
            }
            finally
            {
                _commandsSemaphore.Release();
            }
        }

        private async Task AddLocalEntriesAsync(IEnumerable<AutorouteEntry> entries, bool clear = false)
        {
            await _entriesSemaphore.WaitAsync();
            try
            {
                if (clear)
                {
                    _paths = _paths.Clear();
                    _contentItemIds = _contentItemIds.Clear();
                }

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

        protected virtual async Task InitializeLocalEntriesAsync()
        {
            var indexes = await Session.QueryIndex<AutoroutePartIndex>(i => i.Published).ListAsync();
            var entries = indexes.Select(i => new AutorouteEntry(i.ContentItemId, i.Path, i.ContainedContentItemId, i.JsonPath));
            await AddLocalEntriesAsync(entries, clear: true);
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<AutorouteCommandsDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteCommandsDocument>>();
    }
}
