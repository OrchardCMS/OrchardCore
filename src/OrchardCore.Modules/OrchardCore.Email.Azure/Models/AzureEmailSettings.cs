using System;

namespace OrchardCore.Email.Azure;

/// <summary>
/// Represents a settings for Azure email.
/// </summary>
public class AzureEmailSettings
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via ConnectionStringSecretName.")]
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the Azure Email connection string.
    /// When set, this takes precedence over the ConnectionString property.
    /// </summary>
    public string ConnectionStringSecretName { get; set; }
}
