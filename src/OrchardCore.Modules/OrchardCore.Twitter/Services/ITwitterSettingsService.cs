using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public interface ITwitterSettingsService
    {
        Task<TwitterSettings> GetSettingsAsync();
        Task<TwitterSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(TwitterSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(TwitterSettings settings);
    }
}
