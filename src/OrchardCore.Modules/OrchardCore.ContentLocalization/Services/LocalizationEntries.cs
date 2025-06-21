using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.ContentLocalization.Services;

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
        await EnsureInitializedAsync().ConfigureAwait(false);

        if (_localizations.TryGetValue(contentItemId, out var localization))
        {
            return (true, localization);
        }

        return (false, localization);
    }

    public async Task<IEnumerable<LocalizationEntry>> GetLocalizationsAsync(string localizationSet)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        if (_localizationSets.TryGetValue(localizationSet, out var localizations))
        {
            return localizations;
        }

        return [];
    }

    public async Task UpdateEntriesAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        // Update the cache with a new state and then refresh entries as it would be done on a next request.
        await _localizationStateManager.UpdateAsync(new LocalizationStateDocument(), afterUpdateAsync: RefreshEntriesAsync).ConfigureAwait(false);
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeEntriesAsync().ConfigureAwait(false);
        }
        else
        {
            var state = await _localizationStateManager.GetOrCreateImmutableAsync().ConfigureAwait(false);
            if (_stateIdentifier != state.Identifier)
            {
                await RefreshEntriesAsync(state).ConfigureAwait(false);
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
                localizations = [entry];
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

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_stateIdentifier != state.Identifier)
            {
                var indexes = await Session
                    .QueryIndex<LocalizedContentItemIndex>(i => i.Id > _lastIndexId)
                    .OrderBy(i => i.Id)
                    .ListAsync().ConfigureAwait(false);

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
                        Culture = i.Culture.ToLowerInvariant(),
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

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_initialized)
            {
                var state = await _localizationStateManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

                var indexes = await Session
                    .QueryIndex<LocalizedContentItemIndex>(i => i.Published && i.Culture != null)
                    .OrderBy(i => i.Id)
                    .ListAsync().ConfigureAwait(false);

                var entries = indexes.Select(i => new LocalizationEntry
                {
                    DocumentId = i.DocumentId,
                    ContentItemId = i.ContentItemId,
                    LocalizationSet = i.LocalizationSet,
                    Culture = i.Culture.ToLowerInvariant(),
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
