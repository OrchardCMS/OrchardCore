using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Protects the secrets of steps, such as the password of an FTP server or the token of a web API, with the data
/// protection keys of the tenant. Steps store only protected values, in settings whose names start with
/// <see cref="ProtectedPrefix"/>; editors never render them back. A protected value can't be read on another site, so
/// they aren't exported, and imported pipelines must have their secrets entered again.
/// </summary>
public sealed class DataPipelineSecrets
{
    /// <summary>
    /// The start of the names of the settings that hold protected secrets, such as <c>ProtectedPassword</c>.
    /// </summary>
    public const string ProtectedPrefix = "Protected";

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

    /// <summary>
    /// Removes the protected secrets from settings, such as before exporting them: every property whose name starts
    /// with <see cref="ProtectedPrefix"/>, at any depth.
    /// </summary>
    /// <param name="node">The settings.</param>
    public static void RemoveProtectedValues(JsonNode node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var name in jsonObject.Select(property => property.Key).Where(key => key.StartsWith(ProtectedPrefix, StringComparison.Ordinal)).ToList())
                {
                    jsonObject.Remove(name);
                }

                foreach (var (_, value) in jsonObject)
                {
                    RemoveProtectedValues(value);
                }

                break;

            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    RemoveProtectedValues(item);
                }

                break;
        }
    }
}
