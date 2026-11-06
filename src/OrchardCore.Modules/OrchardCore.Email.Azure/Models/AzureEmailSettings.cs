namespace OrchardCore.Email.Azure;

/// <summary>
/// Represents a settings for Azure email.
/// </summary>
public class AzureEmailSettings
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    /// <summary>
    /// Gets or sets the connection string, protected with Data Protection.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the connection string.
    /// When set, this takes precedence over <see cref="ConnectionString"/>.
    /// </summary>
    public string ConnectionStringSecretName { get; set; }
}
