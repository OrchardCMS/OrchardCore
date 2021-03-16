using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public interface ILocalizationEntries
    {
        Task<(bool, LocalizationEntry)> TryGetLocalizationAsync(string contentItemId);
        Task<IEnumerable<LocalizationEntry>> GetLocalizationsAsync(string localizationSet);
        Task AddEntriesAsync(IEnumerable<LocalizationEntry> entries);
        Task RemoveEntriesAsync(IEnumerable<LocalizationEntry> entries);
    }

    public static class LocalizationEntriesExtensions
    {
        public static Task AddEntryAsync(this ILocalizationEntries entries, LocalizationEntry entry)
            => entries.AddEntriesAsync(new[] { entry });

        public static Task RemoveEntryAsync(this ILocalizationEntries entries, LocalizationEntry entry)
        => entries.RemoveEntriesAsync(new[] { entry });
    }
}
