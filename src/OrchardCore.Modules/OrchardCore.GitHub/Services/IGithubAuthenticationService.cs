using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Services
{
    public interface IGitHubAuthenticationService
    {
        Task<GitHubAuthenticationSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(GitHubAuthenticationSettings settings);
        IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings);
    }
}
