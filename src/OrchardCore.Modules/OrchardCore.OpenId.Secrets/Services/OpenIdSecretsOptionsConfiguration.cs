using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
            var signingKey = GetSecurityKeyAsync(settings.SigningKeySecretName).GetAwaiter().GetResult();
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
            var encryptionKey = GetSecurityKeyAsync(settings.EncryptionKeySecretName).GetAwaiter().GetResult();
            if (encryptionKey != null)
            {
                // Insert at the beginning so it takes precedence
                options.EncryptionCredentials.Insert(0, new EncryptingCredentials(
                    encryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512));
                _logger.LogDebug("OpenID encryption key loaded from secret '{SecretName}'.", settings.EncryptionKeySecretName);
            }
        }
    }

    /// <summary>
    /// Gets a security key from a secret. Supports both RsaKeySecret and X509Secret.
    /// First tries to load as RsaKeySecret, then falls back to X509Secret.
    /// </summary>
    private async Task<SecurityKey> GetSecurityKeyAsync(string secretName)
    {
        try
        {
            // First, try to get as RsaKeySecret (preferred, portable)
            var rsaSecret = await _secretManager.GetSecretAsync<RsaKeySecret>(secretName);
            if (rsaSecret != null)
            {
                return CreateRsaSecurityKey(rsaSecret, secretName);
            }

            // Fall back to X509Secret (certificate store reference)
            var x509Secret = await _secretManager.GetSecretAsync<X509Secret>(secretName);
            if (x509Secret != null)
            {
                return CreateX509SecurityKey(x509Secret, secretName);
            }

            _logger.LogWarning("Secret '{SecretName}' was not found or is not a supported key type (RsaKeySecret or X509Secret).", secretName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load security key from secret '{SecretName}'.", secretName);
            return null;
        }
    }

    private RsaSecurityKey CreateRsaSecurityKey(RsaKeySecret secret, string secretName)
    {
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

    private X509SecurityKey CreateX509SecurityKey(X509Secret secret, string secretName)
    {
        var certificate = secret.GetCertificate();
        if (certificate == null)
        {
            _logger.LogWarning(
                "X509 certificate '{Thumbprint}' was not found in {StoreLocation}/{StoreName} store.",
                secret.Thumbprint, secret.StoreLocation, secret.StoreName);
            return null;
        }

        return new X509SecurityKey(certificate);
    }
}
