using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationEntries : ILocalizationEntries
    {
        private readonly ConcurrentDictionary<string, LocalizationEntry> _localizations;

        public LocalizationEntries()
        {
            _localizations = new ConcurrentDictionary<string, LocalizationEntry>();
        }

        public bool TryGetLocalization(string contentItemId, out LocalizationEntry localization)
        {
            return _localizations.TryGetValue(contentItemId, out localization);
        }

        public IEnumerable<LocalizationEntry> GetLocalizations(string localizationSet)
        {
            return _localizations.Values
                .Where(entry => entry.LocalizationSet == localizationSet)
                .ToArray();
        }

        public void AddEntries(IEnumerable<LocalizationEntry> entries)
        {
            foreach (var entry in entries)
            {
                _localizations[entry.ContentItemId] = entry;
            }
        }

        public void RemoveEntries(IEnumerable<string> contentItemIds)
        {
            foreach (var id in contentItemIds)
            {
                _localizations.TryRemove(id, out var removed);
            }
        }
    }
}
