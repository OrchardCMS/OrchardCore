using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Github.Settings;

namespace OrchardCore.Github.Services
{
    public interface IGithubAuthenticationService
    {
        Task<GithubAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(GithubAuthenticationSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(GithubAuthenticationSettings settings);
    }
}
