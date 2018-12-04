using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services
{
    public interface IFacebookCoreService
    {
        Task<FacebookCoreSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(FacebookCoreSettings settings);
        Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookCoreSettings settings);
    }
}
