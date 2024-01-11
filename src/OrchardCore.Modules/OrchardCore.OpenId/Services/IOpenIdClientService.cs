using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services
{
    public interface IOpenIdClientService
    {
        Task<OpenIdClientSettings> GetSettingsAsync();
        Task<OpenIdClientSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(OpenIdClientSettings settings);
        Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdClientSettings settings);
    }
}
