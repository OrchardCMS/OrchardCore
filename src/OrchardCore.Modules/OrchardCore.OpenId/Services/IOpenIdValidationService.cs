using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services;

public interface IOpenIdValidationService
{
    Task<OpenIdValidationSettings> GetSettingsAsync();
    Task<OpenIdValidationSettings> LoadSettingsAsync();
    /// <summary>
    /// Updates the OpenID validation settings and, once they are committed, refreshes the options built from them
    /// without restarting the tenant.
    /// </summary>
    /// <param name="settings">The updated settings.</param>
    Task UpdateSettingsAsync(OpenIdValidationSettings settings);
    Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdValidationSettings settings);
}
