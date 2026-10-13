using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services;

public interface IOpenIdServerService
{
    Task<OpenIdServerSettings> GetSettingsAsync();
    Task<OpenIdServerSettings> LoadSettingsAsync();
    /// <summary>
    /// Updates the OpenID server settings and, once they are committed, refreshes the options built from them
    /// without restarting the tenant, unless the paths of the endpoints routed to MVC actions have changed,
    /// in which case the tenant is restarted to map the new routes.
    /// </summary>
    /// <param name="settings">The updated settings.</param>
    Task UpdateSettingsAsync(OpenIdServerSettings settings);
    Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdServerSettings settings);
    Task<ImmutableArray<(X509Certificate2 certificate, StoreLocation location, StoreName name)>> GetAvailableCertificatesAsync();
    Task<ImmutableArray<SecurityKey>> GetEncryptionKeysAsync();
    Task<ImmutableArray<SecurityKey>> GetSigningKeysAsync();
    Task PruneManagedCertificatesAsync();
}
