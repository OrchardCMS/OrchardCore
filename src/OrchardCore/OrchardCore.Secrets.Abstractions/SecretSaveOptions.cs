namespace OrchardCore.Secrets;

/// <summary>
/// Options for saving a secret, including metadata.
/// </summary>
public class SecretSaveOptions
{
    /// <summary>
    /// Gets or sets an optional description for the secret.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets when the secret expires. This is informational only and
    /// does not automatically disable the secret. Use for rotation reminders and alerts.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }
}
