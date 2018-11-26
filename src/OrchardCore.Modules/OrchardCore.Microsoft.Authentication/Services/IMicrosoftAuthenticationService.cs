using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IMicrosoftAuthenticationService
    {
        Task<MicrosoftAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(MicrosoftAuthenticationSettings settings);
        Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(MicrosoftAuthenticationSettings settings);
    }
}
