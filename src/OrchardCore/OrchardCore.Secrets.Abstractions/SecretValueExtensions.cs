using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Secrets;

/// <summary>
/// Reads credentials stored either as a protected value in the settings or as a secret of the Secrets module.
/// </summary>
public static class SecretValueExtensions
{
    /// <summary>
    /// Gets the clear value of a credential. A referenced secret takes precedence over the protected value, and
    /// is only read when the Secrets feature is enabled.
    /// </summary>
    /// <param name="services">The services of the current tenant.</param>
    /// <param name="secretName">The name of the referenced <see cref="TextSecret"/>, if any.</param>
    /// <param name="protectedValue">The protected value kept in the settings, if any.</param>
    /// <param name="protector">The protector used for the value kept in the settings.</param>
    /// <param name="logger">An optional logger that receives the reasons why no value could be read. Values are never logged.</param>
    /// <returns>The clear value, or <see langword="null"/> when none is available.</returns>
    public static async Task<string> GetSecretValueAsync(
        this IServiceProvider services,
        string secretName,
        string protectedValue,
        IDataProtector protector,
        ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!string.IsNullOrWhiteSpace(secretName))
        {
            var secretManager = services.GetService<ISecretManager>();

            if (secretManager is null)
            {
                logger?.LogWarning("The secret '{SecretName}' is referenced, but the Secrets feature is not enabled.", secretName);
            }
            else
            {
                var secret = await secretManager.GetSecretAsync<TextSecret>(secretName);

                if (!string.IsNullOrEmpty(secret?.Text))
                {
                    return secret.Text;
                }

                logger?.LogWarning("The secret '{SecretName}' was not found or is empty.", secretName);
            }
        }

        if (string.IsNullOrWhiteSpace(protectedValue) || protector is null)
        {
            return null;
        }

        try
        {
            return protector.Unprotect(protectedValue);
        }
        catch (CryptographicException)
        {
            logger?.LogError("A protected credential could not be decrypted. It may have been encrypted using a different key.");

            return null;
        }
    }
}
