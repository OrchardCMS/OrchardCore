using System.ComponentModel.DataAnnotations;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public interface ITwitterSettingsService
{
    Task<TwitterSettings> GetSettingsAsync();
    Task<TwitterSettings> LoadSettingsAsync();
    Task UpdateSettingsAsync(TwitterSettings settings);
    IEnumerable<ValidationResult> ValidateSettings(TwitterSettings settings);
}
