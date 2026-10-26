namespace OrchardCore.Secrets;

/// <summary>
/// The outcome of moving one credential to a secret.
/// </summary>
public sealed class SecretMigrationResult
{
    /// <summary>
    /// Gets or sets the display name of the credential, for instance <c>Meta: App secret</c>.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret.
    /// </summary>
    public string SecretName { get; set; }

    /// <summary>
    /// Gets or sets whether the credential was moved.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets why the credential could not be moved.
    /// </summary>
    public SecretMigrationError Error { get; set; }
}
