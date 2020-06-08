using System.Collections.Generic;
using System.Collections.Immutable;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationEntries : ILocalizationEntries
    {
        private readonly Dictionary<string, LocalizationEntry> _localizations;
        private readonly Dictionary<string, ImmutableArray<LocalizationEntry>> _localizationSets;

        public LocalizationEntries()
        {
            _localizations = new Dictionary<string, LocalizationEntry>();
            _localizationSets = new Dictionary<string, ImmutableArray<LocalizationEntry>>();
        }

        public bool TryGetLocalization(string contentItemId, out LocalizationEntry localization)
        {
            return _localizations.TryGetValue(contentItemId, out localization);
        }

        public ImmutableArray<LocalizationEntry> GetLocalizations(string localizationSet)
        {
            if (_localizationSets.TryGetValue(localizationSet, out var localizations))
            {
                return localizations;
            }

            return ImmutableArray.Create<LocalizationEntry>();
        }

        public void AddEntries(IEnumerable<LocalizationEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    if (_localizations.ContainsKey(entry.ContentItemId))
                    {
                        continue;
                    }

                    _localizations[entry.ContentItemId] = entry;

                    if (_localizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                    {
                        _localizationSets[entry.LocalizationSet] = localizations.Add(entry);
                    }
                    else
                    {
                        _localizationSets[entry.LocalizationSet] = new[] { entry }.ToImmutableArray();
                    }
                }
            }
        }

        public void RemoveEntries(IEnumerable<LocalizationEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    if (!_localizations.ContainsKey(entry.ContentItemId))
                    {
                        continue;
                    }

                    _localizations.Remove(entry.ContentItemId);

                    if (_localizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                    {
                        _localizationSets[entry.LocalizationSet] = localizations.RemoveAll(l => l.Culture == entry.Culture);
                    }
                }
            }
        }
    }
}
