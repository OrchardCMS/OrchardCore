using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets;

/// <summary>
/// The editor model of a credential, such as a password or an API key, rendered with the <c>&lt;secret-input&gt;</c> tag helper.
/// </summary>
/// <remarks>
/// The credential is either protected and kept in the settings of the feature that uses it, which works without the
/// Secrets feature, or kept in the Secrets module and referenced by its name.
/// </remarks>
public class SecretInputViewModel
{
    /// <summary>
    /// Gets or sets where the credential is kept.
    /// </summary>
    public SecretInputSource Source { get; set; }

    /// <summary>
    /// Gets or sets a new value of the credential. An empty value keeps the current one.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the name of the referenced secret when <see cref="Source"/> is <see cref="SecretInputSource.Secret"/>.
    /// </summary>
    public string SecretName { get; set; }

    /// <summary>
    /// Gets or sets whether a protected value is currently kept in the settings.
    /// </summary>
    [BindNever]
    public bool HasValue { get; set; }

    /// <summary>
    /// Creates the editor model of a credential from the values stored in the settings.
    /// </summary>
    /// <param name="protectedValue">The protected value kept in the settings, if any.</param>
    /// <param name="secretName">The name of the referenced secret, if any.</param>
    public static SecretInputViewModel Create(string protectedValue, string secretName)
        => new()
        {
            Source = string.IsNullOrWhiteSpace(secretName) ? SecretInputSource.Value : SecretInputSource.Secret,
            SecretName = secretName,
            HasValue = !string.IsNullOrWhiteSpace(protectedValue),
        };
}
