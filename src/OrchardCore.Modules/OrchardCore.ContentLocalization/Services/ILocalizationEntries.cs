using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public interface ILocalizationEntries
    {
        Task<(bool, LocalizationEntry)> TryGetLocalizationAsync(string contentItemId);
        Task<IEnumerable<LocalizationEntry>> GetLocalizationsAsync(string localizationSet);
        Task UpdateEntriesAsync();
    }
}
