using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OrchardCore.Secrets;

namespace OrchardCore.OpenId.Secrets;

/// <summary>
/// Reads the RSA keys and X.509 certificates stored in secrets as security keys.
/// </summary>
internal static class OpenIdSecretKeyExtensions
{
    /// <summary>
    /// Gets a security key from a secret. Supports both RsaKeySecret and X509Secret.
    /// First tries to load as RsaKeySecret, then falls back to X509Secret.
    /// </summary>
    public static async Task<SecurityKey> GetSecurityKeyAsync(this ISecretManager secretManager, string secretName, ILogger logger)
    {
        try
        {
            // First, try to get as RsaKeySecret (preferred, portable)
            var rsaSecret = await secretManager.GetSecretAsync<RsaKeySecret>(secretName);
            if (rsaSecret != null)
            {
                return CreateRsaSecurityKey(rsaSecret, secretName, logger);
            }

            // Fall back to X509Secret (certificate store reference or PEM-encoded certificate)
            var x509Secret = await secretManager.GetSecretAsync<X509Secret>(secretName);
            if (x509Secret != null)
            {
                return CreateX509SecurityKey(x509Secret, secretName, logger);
            }

            logger.LogWarning("Secret '{SecretName}' was not found or is not a supported key type (RsaKeySecret or X509Secret).", secretName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load security key from secret '{SecretName}'.", secretName);
            return null;
        }
    }

    private static RsaSecurityKey CreateRsaSecurityKey(RsaKeySecret secret, string secretName, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(secret.PublicKey))
        {
            logger.LogWarning("RSA key secret '{SecretName}' does not contain a public key.", secretName);
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

    private static X509SecurityKey CreateX509SecurityKey(X509Secret secret, string secretName, ILogger logger)
    {
        var certificate = secret.GetCertificate();
        if (certificate == null)
        {
            if (secret.Source == X509SecretSource.Pem)
            {
                logger.LogWarning("The PEM-encoded certificate of the X509 secret '{SecretName}' could not be loaded.", secretName);
            }
            else
            {
                logger.LogWarning(
                    "X509 certificate '{Thumbprint}' was not found in {StoreLocation}/{StoreName} store.",
                    secret.Thumbprint, secret.StoreLocation, secret.StoreName);
            }

            return null;
        }

        return new X509SecurityKey(certificate);
    }
}
