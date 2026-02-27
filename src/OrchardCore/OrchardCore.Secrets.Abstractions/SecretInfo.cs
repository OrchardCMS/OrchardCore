namespace OrchardCore.Secrets;

/// <summary>
/// Provides information about a secret without exposing its value.
/// </summary>
public class SecretInfo
{
    /// <summary>
    /// Gets or sets the name of the secret.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the store containing this secret.
    /// </summary>
    public string Store { get; set; }

    /// <summary>
    /// Gets or sets the type name of the secret.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets when the secret was created.
    /// </summary>
    public DateTime? CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the secret was last updated.
    /// </summary>
    public DateTime? UpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the secret expires. This is informational only and
    /// does not automatically disable the secret. Use for rotation reminders and alerts.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }
}
