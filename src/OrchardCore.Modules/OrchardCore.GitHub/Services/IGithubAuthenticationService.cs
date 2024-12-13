using System.ComponentModel.DataAnnotations;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Services;

public interface IGitHubAuthenticationService
{
    Task<GitHubAuthenticationSettings> GetSettingsAsync();
    Task<GitHubAuthenticationSettings> LoadSettingsAsync();
    Task UpdateSettingsAsync(GitHubAuthenticationSettings settings);
    IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings);
}
