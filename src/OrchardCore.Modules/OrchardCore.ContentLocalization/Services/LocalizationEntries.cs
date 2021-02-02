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
        private ImmutableDictionary<string, LocalizationEntry> _localizations = ImmutableDictionary<string, LocalizationEntry>.Empty;

        private ImmutableDictionary<string, ImmutableList<LocalizationEntry>> _localizationSets =
            ImmutableDictionary<string, ImmutableList<LocalizationEntry>>.Empty;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private int _lastIndexId;
        private string _stateIdentifier;
        private bool _initialized;

        public LocalizationEntries()
        {
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
            await LocalizationStateManager.UpdateAsync(new LocalizationStateDocument(), afterUpdateAsync: RefreshEntriesAsync);
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeEntriesAsync();
            }
            else
            {
                var state = await LocalizationStateManager.GetOrCreateImmutableAsync();
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
                    var indexes = await Session.QueryIndex<LocalizedContentItemIndex>(i => i.Id > _lastIndexId).ListAsync();

                    RemoveEntries(indexes.Where(i => !i.Published)
                        .Select(i => new LocalizationEntry
                        {
                            ContentItemId = i.ContentItemId,
                            LocalizationSet = i.LocalizationSet,
                            Culture = i.Culture.ToLowerInvariant()
                        }));

                    AddEntries(indexes.Where(i => i.Published)
                        .Select(i => new LocalizationEntry
                        {
                            ContentItemId = i.ContentItemId,
                            LocalizationSet = i.LocalizationSet,
                            Culture = i.Culture.ToLowerInvariant()
                        }));

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
                    var state = await LocalizationStateManager.GetOrCreateImmutableAsync();

                    var indexes = await Session.QueryIndex<LocalizedContentItemIndex>(i => i.Published).ListAsync();

                    AddEntries(indexes.Select(i => new LocalizationEntry
                    {
                        ContentItemId = i.ContentItemId,
                        LocalizationSet = i.LocalizationSet,
                        Culture = i.Culture.ToLowerInvariant()
                    }));

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

        private static IVolatileDocumentManager<LocalizationStateDocument> LocalizationStateManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<LocalizationStateDocument>>();
    }
}
