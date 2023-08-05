using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationEntries : ILocalizationEntries
    {
        private readonly IVolatileDocumentManager<LocalizationStateDocument> _localizationStateManager;

        private ImmutableDictionary<string, LocalizationEntry> _localizations = ImmutableDictionary<string, LocalizationEntry>.Empty;

        private ImmutableDictionary<string, ImmutableList<LocalizationEntry>> _localizationSets =
            ImmutableDictionary<string, ImmutableList<LocalizationEntry>>.Empty;

        private readonly SemaphoreSlim _semaphore = new(1);

        private long _lastIndexId;
        private string _stateIdentifier;
        private bool _initialized;

        public LocalizationEntries(IVolatileDocumentManager<LocalizationStateDocument> localizationStateManager)
        {
            _localizationStateManager = localizationStateManager;
        }

        public async Task<(bool, LocalizationEntry)> TryGetLocalizationAsync(string contentItemId)
        {
            await EnsureInitializedAsync();

            if (_localizations.TryGetValue(contentItemId, out var localization))
            {
                return (true, localization);
            }

            return (false, localization);
        }

        public async Task<IEnumerable<LocalizationEntry>> GetLocalizationsAsync(string localizationSet)
        {
            await EnsureInitializedAsync();

            if (_localizationSets.TryGetValue(localizationSet, out var localizations))
            {
                return localizations;
            }

            return Enumerable.Empty<LocalizationEntry>();
        }

        public async Task UpdateEntriesAsync()
        {
            await EnsureInitializedAsync();

            // Update the cache with a new state and then refresh entries as it would be done on a next request.
            await _localizationStateManager.UpdateAsync(new LocalizationStateDocument(), afterUpdateAsync: RefreshEntriesAsync);
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeEntriesAsync();
            }
            else
            {
                var state = await _localizationStateManager.GetOrCreateImmutableAsync();
                if (_stateIdentifier != state.Identifier)
                {
                    await RefreshEntriesAsync(state);
                }
            }
        }

        protected void AddEntries(IEnumerable<LocalizationEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (_localizations.ContainsKey(entry.ContentItemId))
                {
                    continue;
                }

                _localizations = _localizations.SetItem(entry.ContentItemId, entry);

                if (_localizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                {
                    localizations = localizations.Add(entry);
                }
                else
                {
                    localizations = ImmutableList.Create(entry);
                }

                _localizationSets = _localizationSets.SetItem(entry.LocalizationSet, localizations);
            }
        }

        protected void RemoveEntries(IEnumerable<LocalizationEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (!_localizations.ContainsKey(entry.ContentItemId))
                {
                    continue;
                }

                _localizations = _localizations.Remove(entry.ContentItemId);

                if (_localizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                {
                    localizations = localizations.RemoveAll(l => l.Culture == entry.Culture);
                    _localizationSets = _localizationSets.SetItem(entry.LocalizationSet, localizations);
                }
            }
        }

        private async Task RefreshEntriesAsync(LocalizationStateDocument state)
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
                        .QueryIndex<LocalizedContentItemIndex>(i => i.Id > _lastIndexId)
                        .OrderBy(i => i.Id)
                        .ListAsync();

                    // A draft is indexed to check for conflicts, and to remove an entry, but only if an item is unpublished,
                    // so only if the entry 'DocumentId' matches, this because when a draft is saved more than once, the index
                    // is not updated for the published version that may be already scanned, so the entry may not be re-added.

                    var entriesToRemove = indexes
                        .Where(i => !i.Published || i.Culture == null)
                        .SelectMany(i => _localizations.Values.Where(e =>
                            // The item was removed.
                            ((!i.Published && !i.Latest) ||
                            // The part was removed.
                            (i.Culture == null && i.Published) ||
                            // The item was unpublished.
                            (!i.Published && e.DocumentId == i.DocumentId)) &&
                            (e.ContentItemId == i.ContentItemId)));

                    var entriesToAdd = indexes.
                        Where(i => i.Published && i.Culture != null)
                        .Select(i => new LocalizationEntry
                        {
                            DocumentId = i.DocumentId,
                            ContentItemId = i.ContentItemId,
                            LocalizationSet = i.LocalizationSet,
                            Culture = i.Culture.ToLowerInvariant()
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
                    var state = await _localizationStateManager.GetOrCreateImmutableAsync();

                    var indexes = await Session
                        .QueryIndex<LocalizedContentItemIndex>(i => i.Published && i.Culture != null)
                        .OrderBy(i => i.Id)
                        .ListAsync();

                    var entries = indexes.Select(i => new LocalizationEntry
                    {
                        DocumentId = i.DocumentId,
                        ContentItemId = i.ContentItemId,
                        LocalizationSet = i.LocalizationSet,
                        Culture = i.Culture.ToLowerInvariant()
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
