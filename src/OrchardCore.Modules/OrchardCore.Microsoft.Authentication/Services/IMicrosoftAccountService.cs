using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public interface IMicrosoftAccountService
    {
        Task<MicrosoftAccountSettings> GetSettingsAsync();
        Task<MicrosoftAccountSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(MicrosoftAccountSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(MicrosoftAccountSettings settings);
    }
}
