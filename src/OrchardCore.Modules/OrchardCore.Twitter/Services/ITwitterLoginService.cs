using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public interface ITwitterLoginService
    {
        Task<TwitterLoginSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(TwitterLoginSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(TwitterLoginSettings settings);
    }
}
