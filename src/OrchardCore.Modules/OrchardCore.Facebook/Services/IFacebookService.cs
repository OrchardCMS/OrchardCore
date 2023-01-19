using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IFacebookService
    {
        Task<FacebookSettings> GetSettingsAsync();

        Task UpdateSettingsAsync(FacebookSettings settings);

        IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings);
    }
}
