using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Secrets.Services;

/// <summary>
/// Post-configures OpenIddict server options to use secrets for signing and encryption keys.
/// </summary>
public sealed class OpenIdSecretsOptionsConfiguration : IPostConfigureOptions<OpenIddictServerOptions>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public OpenIdSecretsOptionsConfiguration(
        ISiteService siteService,
        ISecretManager secretManager,
        ILogger<OpenIdSecretsOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void PostConfigure(string name, OpenIddictServerOptions options)
    {
        var settings = _siteService.GetSettings<OpenIdSecretSettings>();

        // Configure signing key from secrets
        if (!string.IsNullOrWhiteSpace(settings?.SigningKeySecretName))
        {
            var signingKey = GetRsaSecurityKeyAsync(settings.SigningKeySecretName).GetAwaiter().GetResult();
            if (signingKey != null)
            {
                // Insert at the beginning so it takes precedence
                options.SigningCredentials.Insert(0, new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256));
                _logger.LogDebug("OpenID signing key loaded from secret '{SecretName}'.", settings.SigningKeySecretName);
            }
        }

        // Configure encryption key from secrets
        if (!string.IsNullOrWhiteSpace(settings?.EncryptionKeySecretName))
        {
            var encryptionKey = GetRsaSecurityKeyAsync(settings.EncryptionKeySecretName).GetAwaiter().GetResult();
            if (encryptionKey != null)
            {
                // Insert at the beginning so it takes precedence
                options.EncryptionCredentials.Insert(0, new EncryptingCredentials(
                    encryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512));
                _logger.LogDebug("OpenID encryption key loaded from secret '{SecretName}'.", settings.EncryptionKeySecretName);
            }
        }
    }

    private async Task<RsaSecurityKey> GetRsaSecurityKeyAsync(string secretName)
    {
        try
        {
            var secret = await _secretManager.GetSecretAsync<RsaKeySecret>(secretName);

            if (secret == null)
            {
                _logger.LogWarning("RSA key secret '{SecretName}' was not found.", secretName);
                return null;
            }

            if (string.IsNullOrWhiteSpace(secret.PublicKey))
            {
                _logger.LogWarning("RSA key secret '{SecretName}' does not contain a public key.", secretName);
                return null;
            }

            var rsa = RSA.Create();

            // Import the public key
            rsa.ImportRSAPublicKey(Convert.FromBase64String(secret.PublicKey), out _);

            // Import the private key if available
            if (secret.IncludesPrivateKey && !string.IsNullOrWhiteSpace(secret.PrivateKey))
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(secret.PrivateKey), out _);
            }

            return new RsaSecurityKey(rsa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load RSA key from secret '{SecretName}'.", secretName);
            return null;
        }
    }
}
