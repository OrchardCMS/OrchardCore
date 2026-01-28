namespace OrchardCore.Secrets.Models;

/// <summary>
/// Represents a stored secret entry in the database.
/// </summary>
public class SecretEntry
{
    /// <summary>
    /// Gets or sets the name of the secret.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified type name of the secret.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the encrypted secret data as JSON.
    /// </summary>
    public string EncryptedData { get; set; }

    /// <summary>
    /// Gets or sets when the secret was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the secret was last updated.
    /// </summary>
    public DateTime UpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the secret expires. This is informational only and
    /// does not automatically disable the secret. Use for rotation reminders and alerts.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets an optional description for the secret.
    /// </summary>
    public string Description { get; set; }
}
