using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public interface IMicrosoftAuthenticationService
    {
        Task<MicrosoftAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(MicrosoftAuthenticationSettings settings);
        Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(MicrosoftAuthenticationSettings settings);
    }
}
