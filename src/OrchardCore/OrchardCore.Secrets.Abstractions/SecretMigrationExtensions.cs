using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace OrchardCore.Secrets;

/// <summary>
/// Helps display drivers of <see cref="SecretMigration"/> move credentials kept in settings to secrets.
/// </summary>
public static class SecretMigrationExtensions
{
    /// <summary>
    /// Decrypts a protected value and saves it as a <see cref="TextSecret"/> in the store selected for the migration,
    /// then records the outcome in <see cref="SecretMigration.Results"/>.
    /// </summary>
    /// <param name="migration">The current migration.</param>
    /// <param name="secretManager">The secret manager.</param>
    /// <param name="protector">The protector used for the value kept in the settings.</param>
    /// <param name="protectedValue">The protected value kept in the settings.</param>
    /// <param name="secretName">The name of the secret to create. An existing secret is never overwritten.</param>
    /// <param name="displayName">The display name of the credential, used in the results and as the secret description.</param>
    /// <returns><see langword="true"/> when the secret was saved and the settings can reference it; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> MoveToSecretAsync(
        this SecretMigration migration,
        ISecretManager secretManager,
        IDataProtector protector,
        string protectedValue,
        string secretName,
        string displayName)
    {
        ArgumentNullException.ThrowIfNull(migration);
        ArgumentNullException.ThrowIfNull(secretManager);
        ArgumentNullException.ThrowIfNull(protector);

        secretName = secretName?.Trim();

        var result = new SecretMigrationResult
        {
            DisplayName = displayName,
            SecretName = secretName,
        };

        migration.Results.Add(result);

        if (string.IsNullOrEmpty(secretName))
        {
            result.Error = SecretMigrationError.MissingName;

            return false;
        }

        var infos = await secretManager.GetSecretInfosAsync();

        if (infos.Any(info => string.Equals(info.Name, secretName, StringComparison.OrdinalIgnoreCase)))
        {
            result.Error = SecretMigrationError.AlreadyExists;

            return false;
        }

        string value;

        try
        {
            value = protector.Unprotect(protectedValue);
        }
        catch (CryptographicException)
        {
            result.Error = SecretMigrationError.DecryptionFailed;

            return false;
        }

        var options = new SecretSaveOptions
        {
            Description = displayName,
        };

        try
        {
            if (string.IsNullOrEmpty(migration.Store))
            {
                await secretManager.SaveSecretAsync(secretName, new TextSecret { Text = value }, options);
            }
            else
            {
                await secretManager.SaveSecretAsync(secretName, new TextSecret { Text = value }, migration.Store, options);
            }
        }
        catch (Exception)
        {
            result.Error = SecretMigrationError.StoreFailed;

            return false;
        }

        result.Succeeded = true;

        return true;
    }
}
