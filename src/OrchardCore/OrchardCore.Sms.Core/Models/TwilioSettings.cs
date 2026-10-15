namespace OrchardCore.Sms.Models;

public class TwilioSettings
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    /// <summary>
    /// Gets or sets the auth token, protected with Data Protection.
    /// </summary>
    public string AuthToken { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the auth token.
    /// When set, this takes precedence over <see cref="AuthToken"/>.
    /// </summary>
    public string AuthTokenSecretName { get; set; }
}
