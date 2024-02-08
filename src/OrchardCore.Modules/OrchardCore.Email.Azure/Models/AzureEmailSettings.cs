namespace OrchardCore.Email.Azure;

/// <summary>
/// Represents a settings for Azure email.
/// </summary>
public class AzureEmailSettings : EmailProviderSettings
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; }
}
