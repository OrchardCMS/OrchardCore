using System.ComponentModel.DataAnnotations;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Services;

public interface IGoogleAuthenticationService
{
    Task<GoogleAuthenticationSettings> GetSettingsAsync();

    Task UpdateSettingsAsync(GoogleAuthenticationSettings settings);

    IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings);
}
