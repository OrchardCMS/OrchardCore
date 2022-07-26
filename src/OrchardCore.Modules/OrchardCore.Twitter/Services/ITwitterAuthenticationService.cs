using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public interface ITwitterAuthenticationService
{
    Task<TwitterAuthenticationSettings> GetSettingsAsync();

    Task<TwitterAuthenticationSettings> LoadSettingsAsync();

    Task UpdateSettingsAsync(TwitterAuthenticationSettings settings);

    IEnumerable<ValidationResult> ValidateSettings(TwitterAuthenticationSettings settings);
}
