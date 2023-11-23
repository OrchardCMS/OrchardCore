namespace OrchardCore.Email.Azure
{
    /// <summary>
    /// Represents a settings for Azure email.
    /// </summary>
    public class AzureEmailSettings : EmailSettings
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
