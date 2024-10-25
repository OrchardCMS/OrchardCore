namespace OrchardCore.Email;

/// <summary>
/// Represents a settings for SMTP.
/// </summary>
public class SmtpSettings : EmailProviderSettings
{
    /// <summary>
    /// Gets or sets the SMTP server/host.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the SMTP port number. Defaults to <c>25</c>.
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// Gets or sets whether the encryption is automatically selected.
    /// </summary>
    public bool AutoSelectEncryption { get; set; }

    /// <summary>
    /// Gets or sets whether the user credentials is required.
    /// </summary>
    public bool RequireCredentials { get; set; }

    /// <summary>
    /// Gets or sets whether to use the default user credentials.
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Gets or sets the mail encryption method.
    /// </summary>
    public SmtpEncryptionMethod EncryptionMethod { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the user password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the proxy server.
    /// </summary>
    public string ProxyHost { get; set; }

    /// <summary>
    /// Gets or sets the proxy port number.
    /// </summary>
    public int ProxyPort { get; set; }

    /// <summary>
    /// Gets or sets whether invalid SSL certificates should be ignored.
    /// </summary>
    public bool IgnoreInvalidSslCertificate { get; set; }

    /// <summary>
    /// Gets or sets the mail delivery method.
    /// </summary>
    public SmtpDeliveryMethod DeliveryMethod { get; set; }

    /// <summary>
    /// Gets or sets the mailbox directory, this used for <see cref="SmtpDeliveryMethod.SpecifiedPickupDirectory"/> option.
    /// </summary>
    public string PickupDirectoryLocation { get; set; }
}
