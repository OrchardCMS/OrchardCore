using System.ComponentModel.DataAnnotations;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services;

public interface IFacebookService
{
    Task<FacebookSettings> GetSettingsAsync();

    Task UpdateSettingsAsync(FacebookSettings settings);

    IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings);
}
