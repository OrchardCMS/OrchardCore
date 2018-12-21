using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Google.Settings;

namespace OrchardCore.Google.Services
{
    public interface IGoogleAuthenticationService
    {
        Task<GoogleAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(GoogleAuthenticationSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings);
    }
}
