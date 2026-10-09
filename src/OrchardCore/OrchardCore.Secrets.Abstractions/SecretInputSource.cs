namespace OrchardCore.Secrets;

/// <summary>
/// Describes where the value of a credential edited through a <see cref="SecretInputViewModel"/> is kept.
/// </summary>
public enum SecretInputSource
{
    /// <summary>
    /// The value is entered in the settings, protected with Data Protection, and kept with them. This is the default,
    /// and the only option available when the Secrets feature is disabled.
    /// </summary>
    Value,

    /// <summary>
    /// The settings reference a secret of the Secrets module by its name.
    /// </summary>
    Secret,
}
