using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public interface IAzureADService
    {
        Task<AzureADSettings> GetSettingsAsync();
        Task<AzureADSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(AzureADSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(AzureADSettings settings);
    }
}
