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
    public string ConnectionString { get; set; }
}
