using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IAzureADAuthenticationService
    {
        Task<AzureADAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(AzureADAuthenticationSettings settings);
        Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(AzureADAuthenticationSettings settings);
    }
}
