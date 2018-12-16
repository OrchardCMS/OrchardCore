using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public interface ITwitterSigninService
    {
        Task<TwitterSigninSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(TwitterSigninSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(TwitterSigninSettings settings);
    }
}
