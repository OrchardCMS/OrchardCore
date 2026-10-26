namespace OrchardCore.Secrets;

/// <summary>
/// Describes why a credential could not be moved to a secret.
/// </summary>
public enum SecretMigrationError
{
    /// <summary>
    /// The credential was moved.
    /// </summary>
    None,

    /// <summary>
    /// No name was given to the secret.
    /// </summary>
    MissingName,

    /// <summary>
    /// A secret with the same name already exists. Existing secrets are never overwritten.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The protected value could not be decrypted, for instance because the Data Protection keys changed.
    /// </summary>
    DecryptionFailed,

    /// <summary>
    /// The secret store could not save the secret. The settings were left unchanged.
    /// </summary>
    StoreFailed,
}
