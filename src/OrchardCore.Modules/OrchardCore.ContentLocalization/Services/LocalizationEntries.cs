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

        private string _identifier;
        private uint _lastIndexId;
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
            await DocumentManager.UpdateAsync(new LocalizationStateDocument(), afterUpdateAsync: RefreshEntriesAsync);
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeEntriesAsync();
            }
            else
            {
                var document = await DocumentManager.GetOrCreateImmutableAsync();
                if (_identifier != document.Identifier)
                {
                    await RefreshEntriesAsync(document);
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

        private async Task RefreshEntriesAsync(LocalizationStateDocument document)
        {
            if (_identifier == document.Identifier)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_identifier != document.Identifier)
                {
                    var indexes = await Session.QueryIndex<LocalizedContentItemIndex>(i => (uint)i.Id > _lastIndexId).ListAsync();

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

                    _lastIndexId = (uint)(indexes.LastOrDefault()?.Id ?? 0);
                    _identifier = document.Identifier;
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
                    var document = await DocumentManager.GetOrCreateImmutableAsync();

                    // Todo: check if we now need more conditions
                    var indexes = await Session.QueryIndex<LocalizedContentItemIndex>(i => i.Published).ListAsync();

                    AddEntries(indexes.Select(i => new LocalizationEntry
                    {
                        ContentItemId = i.ContentItemId,
                        LocalizationSet = i.LocalizationSet,
                        Culture = i.Culture.ToLowerInvariant()
                    }));

                    _lastIndexId = (uint)(indexes.LastOrDefault()?.Id ?? 0);
                    _identifier = document.Identifier;

                    _initialized = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<LocalizationStateDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<LocalizationStateDocument>>();
    }
}
