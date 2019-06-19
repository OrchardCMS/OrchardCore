using System.Collections.Generic;
using System.Collections.Immutable;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public interface ILocalizationEntries
    {
        bool TryGetLocalization(string contentItemId, out LocalizationEntry localization);
        ImmutableArray<LocalizationEntry> GetLocalizations(string localizationSet);
        void AddEntries(IEnumerable<LocalizationEntry> entries);
        void RemoveEntries(IEnumerable<LocalizationEntry> entries);
    }

    public static class LocalizationEntriesExtensions
    {
        public static void AddEntry(this ILocalizationEntries entries, LocalizationEntry entry)
        {
            entries.AddEntries(new[] { entry });
        }

        public static void RemoveEntry(this ILocalizationEntries entries, LocalizationEntry entry)
        {
            entries.RemoveEntries(new[] { entry });
        }
    }
}
