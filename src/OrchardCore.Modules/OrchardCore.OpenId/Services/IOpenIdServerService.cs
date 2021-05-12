using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services
{
    public interface IOpenIdServerService
    {
        Task<OpenIdServerSettings> GetSettingsAsync();
        Task<OpenIdServerSettings> LoadSettingsAsync();
        Task UpdateSettingsAsync(OpenIdServerSettings settings);
        Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdServerSettings settings);
        Task<ImmutableArray<(X509Certificate2 certificate, StoreLocation location, StoreName name)>> GetAvailableCertificatesAsync();
        Task<ImmutableArray<SecurityKey>> GetEncryptionKeysAsync();
        Task<ImmutableArray<SecurityKey>> GetSigningKeysAsync();
        Task PruneManagedCertificatesAsync();
    }
}
