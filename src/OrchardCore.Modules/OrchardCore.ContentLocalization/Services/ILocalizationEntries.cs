using System.Collections.Generic;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public interface ILocalizationEntries
    {
        bool TryGetLocalization(string contentItemId, out LocalizationEntry localization);
        IEnumerable<LocalizationEntry> GetLocalizations(string localizationSet);
        void AddEntries(IEnumerable<LocalizationEntry> entries);
        void RemoveEntries(IEnumerable<string> contentItemIds);
    }

    public static class LocalizationEntriesExtensions
    {
        public static void AddEntry(this ILocalizationEntries entries, LocalizationEntry entry)
        {
            entries.AddEntries(new[] { entry });
        }

        public static void RemoveEntry(this ILocalizationEntries entries, string contentItemId)
        {
            entries.RemoveEntries(new[] { contentItemId });
        }
    }
}
