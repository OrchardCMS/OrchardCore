using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services
{
    public interface IOpenIdValidationService
    {
        Task<OpenIdValidationSettings> GetSettingsAsync();
        Task<OpenIdValidationSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(OpenIdValidationSettings settings);
        Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdValidationSettings settings);
    }
}
