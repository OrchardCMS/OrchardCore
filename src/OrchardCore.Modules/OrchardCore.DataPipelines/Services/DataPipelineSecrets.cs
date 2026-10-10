using Microsoft.AspNetCore.DataProtection;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Protects the secrets of steps, such as the password of an FTP server or the token of a web API, with the data
/// protection keys of the tenant. Steps store only protected values; editors never render them back. A protected
/// value can't be read on another site, so exported pipelines must have their secrets entered again.
/// </summary>
public sealed class DataPipelineSecrets
{
    private const string Purpose = "OrchardCore.DataPipelines.Secrets";

    private readonly IDataProtector _protector;

    public DataPipelineSecrets(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    /// <summary>
    /// Protects a secret.
    /// </summary>
    /// <param name="secret">The secret.</param>
    /// <returns>The protected value, or <see langword="null"/> for an empty secret.</returns>
    public string Protect(string secret)
        => string.IsNullOrEmpty(secret) ? null : _protector.Protect(secret);

    /// <summary>
    /// Reads a protected secret.
    /// </summary>
    /// <param name="protectedValue">The protected value.</param>
    /// <returns>The secret, or <see langword="null"/> when there is none or it can't be read, such as when it was
    /// protected on another site.</returns>
    public string Unprotect(string protectedValue)
    {
        if (string.IsNullOrEmpty(protectedValue))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(protectedValue);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }

    /// <summary>
    /// Applies a secret posted by an editor: an empty value keeps the current secret, unless it is to be cleared.
    /// </summary>
    /// <param name="current">The current protected value.</param>
    /// <param name="posted">The posted secret.</param>
    /// <param name="clear">Whether the user asked to remove the secret.</param>
    /// <returns>The new protected value.</returns>
    public string Update(string current, string posted, bool clear = false)
    {
        if (clear)
        {
            return null;
        }

        return string.IsNullOrEmpty(posted) ? current : Protect(posted);
    }
}
