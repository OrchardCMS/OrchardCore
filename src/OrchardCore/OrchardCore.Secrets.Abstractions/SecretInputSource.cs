namespace OrchardCore.Secrets;

/// <summary>
/// Describes where the value entered through a <see cref="SecretInputViewModel"/> is kept.
/// </summary>
public enum SecretInputSource
{
    /// <summary>
    /// The value is protected with Data Protection and kept in the settings of the feature that uses it.
    /// This is the only option available when the Secrets feature is disabled.
    /// </summary>
    Value,

    /// <summary>
    /// The settings reference an existing secret of the Secrets module by its name.
    /// </summary>
    Secret,

    /// <summary>
    /// The entered value, or the value currently kept in the settings, is saved as a new secret
    /// of the Secrets module, and the settings reference it.
    /// </summary>
    NewSecret,
}
